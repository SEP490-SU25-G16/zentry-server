using System.Text.RegularExpressions;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zentry.Modules.ScheduleManagement.Domain.Entities;
using Zentry.SharedKernel.Domain;
using Zentry.SharedKernel.Enums.Schedule;

namespace Zentry.Modules.ScheduleManagement.Infrastructure.Persistence.SeedData;

public static class ScheduleSeedData
{
    private const int NumCoursesToGenerate = 35;
    private const int NumRoomsToGenerate = 20;
    private const int NumClassSectionsToGenerate = 45;
    private const int MinSchedulesPerClassSection = 4;
    private const int MaxSchedulesPerClassSection = 6;
    private const int MaxAttemptsForUniqueSchedule = 10;
    private const int NumEnrollmentsToGenerate = 500;
    private static List<Course> SeededCourses { get; set; } = [];
    private static List<Room> SeededRooms { get; set; } = [];
    private static List<ClassSection> SeededClassSections { get; set; } = [];
    private static List<Schedule> SeededSchedules { get; set; } = [];
    private static List<Enrollment> SeededEnrollments { get; set; } = [];

    // **LOẠI BỎ CÁC PHƯƠNG THỨC GetSeededScheduleDtos() và GetSeededClassSectionDtos() Ở ĐÂY**

    public static async Task SeedCoursesAsync(ScheduleDbContext context, ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger?.LogInformation("Starting Course seed data...");

            if (await context.Courses.AnyAsync(cancellationToken))
            {
                logger?.LogInformation("Course data already exists. Skipping seed.");
                SeededCourses =
                    await context.Courses.AsNoTracking().ToListAsync(cancellationToken);
                return;
            }

            Randomizer.Seed = new Random(200);

            var courseFaker = new Faker<Course>()
                .CustomInstantiator(f =>
                {
                    var subjectPrefix = Regex.Replace(f.Random.Word().Substring(0, Math.Min(3, f.Random.Word().Length)),
                        "[^a-zA-Z]", "").ToUpper();
                    if (subjectPrefix.Length < 2) subjectPrefix = f.Random.String2(2, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");

                    var code = $"{subjectPrefix}{f.Random.Number(100, 499)}";

                    var name = f.Commerce.ProductAdjective() + " " + f.Hacker.Noun() + " " + f.Random.Word();
                    name = char.ToUpper(name[0]) + name.Substring(1);

                    return Course.Create(
                        code,
                        name,
                        f.Lorem.Sentence(5, 10)
                    );
                });

            SeededCourses = courseFaker.Generate(NumCoursesToGenerate);
            await context.Courses.AddRangeAsync(SeededCourses, cancellationToken);
            logger?.LogInformation($"Added {SeededCourses.Count} Courses.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while seeding Course data");
            throw;
        }
    }

    public static async Task SeedRoomsAsync(ScheduleDbContext context, ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger?.LogInformation("Starting Room seed data...");

            if (await context.Rooms.AnyAsync(cancellationToken))
            {
                logger?.LogInformation("Room data already exists. Skipping seed.");
                SeededRooms = await context.Rooms.AsNoTracking().ToListAsync(cancellationToken);
                return;
            }

            Randomizer.Seed = new Random(201);

            var uniqueRoomNames = new HashSet<string>();
            var faker = new Faker();
            var roomPrefixes = new[] { "A", "B", "C", "D", "E", "F", "G", "H" };
            var roomNumbers = Enumerable.Range(101, 500).ToList();

            faker.Random.Shuffle(roomNumbers);

            var currentRoomNumberIndex = 0;

            while (SeededRooms.Count < NumRoomsToGenerate)
            {
                if (currentRoomNumberIndex >= roomNumbers.Count)
                {
                    logger?.LogWarning(
                        "Not enough unique room number combinations available. Stopping room generation.");
                    break;
                }

                var building = faker.PickRandom(roomPrefixes);
                var roomNum = roomNumbers[currentRoomNumberIndex];
                currentRoomNumberIndex++;

                var roomName = $"Room {building}{roomNum}";

                if (!uniqueRoomNames.Add(roomName)) continue;
                var room = Room.Create(
                    roomName,
                    $"Building {building}",
                    faker.Random.Number(20, 150)
                );
                SeededRooms.Add(room);
            }

            if (SeededRooms.Count > 0)
            {
                await context.Rooms.AddRangeAsync(SeededRooms, cancellationToken);
                logger?.LogInformation($"Added {SeededRooms.Count} Rooms.");
            }
            else
            {
                logger?.LogWarning("No rooms were generated or added during seeding.");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while seeding Room data");
            throw;
        }
    }

    public static async Task SeedClassSectionsAsync(ScheduleDbContext context, List<Guid> lecturerIds,
        ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        try
        {
            logger?.LogInformation("Starting ClassSection seed data...");

            if (SeededCourses.Count == 0)
            {
                logger?.LogWarning("No Courses available to create ClassSections. Please seed Courses first.");
                return;
            }

            if (lecturerIds.Count == 0)
            {
                logger?.LogWarning(
                    "No Lecturers (User IDs) available to create ClassSections. Please seed Lecturers in User Management first.");
                return;
            }

            if (await context.ClassSections.AnyAsync(cancellationToken))
            {
                logger?.LogInformation("ClassSection data already exists. Skipping seed.");
                SeededClassSections = await context.ClassSections.AsNoTracking().ToListAsync(cancellationToken);
                return;
            }

            Randomizer.Seed = new Random(202);
            var faker = new Faker();

            var semesters = new[] { "SP25", "SU25", "FA25", "SP26", "SU26" };
            var classSectionsToAdd = new List<ClassSection>();

            var uniqueSectionCodePool = new HashSet<string>();
            var maxSectionSuffix = 20;

            foreach (var course in SeededCourses)
                for (var suffix = 1; suffix <= maxSectionSuffix; suffix++)
                    uniqueSectionCodePool.Add($"{course.Code}-{suffix:D2}");

            var shuffledUniqueSectionCodes = uniqueSectionCodePool.ToList();
            faker.Random.Shuffle(shuffledUniqueSectionCodes);

            var actualNumClassSectionsToGenerate =
                Math.Min(NumClassSectionsToGenerate, shuffledUniqueSectionCodes.Count);

            for (var i = 0; i < actualNumClassSectionsToGenerate; i++)
            {
                var sectionCode = shuffledUniqueSectionCodes[i];
                var matchingCourses = SeededCourses.Where(c => sectionCode.StartsWith($"{c.Code}-")).ToList();
                var randomCourse = matchingCourses.Any()
                    ? faker.PickRandom(matchingCourses)
                    : faker.PickRandom(SeededCourses);

                var randomLecturerId = faker.PickRandom(lecturerIds);
                var randomSemester = faker.PickRandom(semesters);

                var classSection = ClassSection.Create(
                    randomCourse.Id,
                    randomLecturerId,
                    sectionCode,
                    randomSemester
                );
                classSectionsToAdd.Add(classSection);
            }

            SeededClassSections = classSectionsToAdd;
            await context.ClassSections.AddRangeAsync(SeededClassSections, cancellationToken);
            logger?.LogInformation($"Added {SeededClassSections.Count} ClassSections.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while seeding ClassSection data");
            throw;
        }
    }

    public static async Task SeedSchedulesAsync(ScheduleDbContext context, ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger?.LogInformation("Starting Schedule seed data...");

            if (SeededClassSections.Count == 0)
            {
                logger?.LogWarning("No ClassSections available to create Schedules. Please seed ClassSections first.");
                return;
            }

            if (SeededRooms.Count == 0)
            {
                logger?.LogWarning("No Rooms available to create Schedules. Please seed Rooms first.");
                return;
            }

            if (await context.Schedules.AnyAsync(cancellationToken))
            {
                logger?.LogInformation("Schedule data already exists. Skipping seed.");
                SeededSchedules = await context.Schedules.AsNoTracking().ToListAsync(cancellationToken);
                return;
            }

            Randomizer.Seed = new Random(203);
            var faker = new Faker();

            var schedulesToAdd = new List<Schedule>();
            var occupiedSlots = new HashSet<(Guid roomId, DateOnly date, TimeOnly startTime, TimeOnly endTime)>();
            var lecturerOccupiedSlots =
                new HashSet<(Guid lecturerId, DateOnly date, TimeOnly startTime, TimeOnly endTime)>();

            var commonTimeSlots = new List<(TimeOnly Start, TimeOnly End)>
            {
                (new TimeOnly(8, 0), new TimeOnly(9, 30)),
                (new TimeOnly(9, 45), new TimeOnly(11, 15)),
                (new TimeOnly(13, 0), new TimeOnly(14, 30)),
                (new TimeOnly(14, 45), new TimeOnly(16, 15)),
                (new TimeOnly(16, 30), new TimeOnly(18, 0))
            };

            var weekdays = Enumeration.GetAll<WeekDayEnum>().ToList();

            var currentYear = DateTime.Now.Year;
            var semesters = new Dictionary<string, (DateOnly Start, DateOnly End)>
            {
                { "SP25", (new DateOnly(2025, 1, 15), new DateOnly(2025, 5, 15)) },
                { "SU25", (new DateOnly(2025, 6, 1), new DateOnly(2025, 8, 30)) },
                { "FA25", (new DateOnly(2025, 9, 1), new DateOnly(2025, 12, 30)) },
                { "SP26", (new DateOnly(2026, 1, 15), new DateOnly(2026, 5, 15)) },
                { "SU26", (new DateOnly(2026, 6, 1), new DateOnly(2026, 8, 30)) }
            };


            foreach (var classSection in SeededClassSections)
            {
                var numSchedules = faker.Random.Number(MinSchedulesPerClassSection, MaxSchedulesPerClassSection);
                var sectionSemesterDates = semesters[classSection.Semester];

                for (var i = 0; i < numSchedules; i++)
                {
                    var scheduleCreated = false;
                    for (var attempt = 0; attempt < MaxAttemptsForUniqueSchedule; attempt++)
                    {
                        var randomRoom = faker.PickRandom(SeededRooms);
                        var randomTimeSlot = faker.PickRandom(commonTimeSlots);
                        var randomWeekDay =
                            faker.PickRandom(weekdays);

                        var scheduleDate =
                            faker.Date.BetweenDateOnly(sectionSemesterDates.Start, sectionSemesterDates.End);

                        while (scheduleDate.DayOfWeek != (DayOfWeek)randomWeekDay.Id)
                        {
                            scheduleDate = scheduleDate.AddDays(1);
                            if (scheduleDate > sectionSemesterDates.End) break;
                        }

                        if (scheduleDate > sectionSemesterDates.End) continue;

                        var roomConflict = occupiedSlots.Any(slot =>
                            slot.roomId == randomRoom.Id &&
                            slot.date == scheduleDate &&
                            randomTimeSlot.Start < slot.endTime && randomTimeSlot.End > slot.startTime);

                        var lecturerConflict = lecturerOccupiedSlots.Any(slot =>
                            slot.lecturerId == classSection.LecturerId &&
                            slot.date == scheduleDate &&
                            randomTimeSlot.Start < slot.endTime && randomTimeSlot.End > slot.startTime);

                        if (!roomConflict && !lecturerConflict)
                        {
                            var newSchedule = Schedule.Create(
                                classSection.Id,
                                randomRoom.Id,
                                scheduleDate,
                                scheduleDate, // Assuming single-day schedules for simplicity in seed
                                randomTimeSlot.Start,
                                randomTimeSlot.End,
                                randomWeekDay
                            );
                            schedulesToAdd.Add(newSchedule);

                            occupiedSlots.Add((randomRoom.Id, scheduleDate, randomTimeSlot.Start, randomTimeSlot.End));
                            lecturerOccupiedSlots.Add((classSection.LecturerId, scheduleDate, randomTimeSlot.Start,
                                randomTimeSlot.End));
                            scheduleCreated = true;
                            break;
                        }
                    }

                    if (!scheduleCreated)
                        logger?.LogWarning(
                            $"Could not find a unique time slot for ClassSection {classSection.SectionCode} after {MaxAttemptsForUniqueSchedule} attempts.");
                }
            }

            SeededSchedules = schedulesToAdd;
            await context.Schedules.AddRangeAsync(SeededSchedules, cancellationToken);
            logger?.LogInformation($"Added {SeededSchedules.Count} Schedules.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while seeding Schedule data");
            throw;
        }
    }

    public static async Task SeedEnrollmentsAsync(ScheduleDbContext context, List<Guid> studentIds,
        ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        try
        {
            logger?.LogInformation("Starting Enrollment seed data...");

            if (studentIds.Count == 0)
            {
                logger?.LogWarning(
                    "No Students (User IDs) available to create Enrollments. Please seed Students in User Management first.");
                return;
            }

            if (SeededClassSections.Count == 0)
            {
                logger?.LogWarning(
                    "No ClassSections available to create Enrollments. Please seed ClassSections first.");
                return;
            }

            if (await context.Enrollments.AnyAsync(cancellationToken))
            {
                logger?.LogInformation("Enrollment data already exists. Skipping seed.");
                SeededEnrollments = await context.Enrollments.AsNoTracking().ToListAsync(cancellationToken);
                return;
            }

            Randomizer.Seed = new Random(204);
            var faker = new Faker();

            var enrollmentsToAdd = new List<Enrollment>();
            var uniqueEnrollments = new HashSet<(Guid StudentId, Guid ClassSectionId)>();

            var attempts = 0;
            const int maxTotalAttempts = 5000;

            while (enrollmentsToAdd.Count < NumEnrollmentsToGenerate && attempts < maxTotalAttempts)
            {
                var randomStudentId = faker.PickRandom(studentIds);
                var randomClassSection = faker.PickRandom(SeededClassSections);

                if (uniqueEnrollments.Add((randomStudentId, randomClassSection.Id)))
                {
                    var enrollment = Enrollment.Create(randomStudentId, randomClassSection.Id);
                    enrollmentsToAdd.Add(enrollment);
                }

                attempts++;
            }

            if (enrollmentsToAdd.Count > 0)
            {
                SeededEnrollments = enrollmentsToAdd;
                await context.Enrollments.AddRangeAsync(SeededEnrollments, cancellationToken);
                logger?.LogInformation($"Added {SeededEnrollments.Count} Enrollments.");
            }
            else
            {
                logger?.LogWarning("No Enrollments were generated or added during seeding.");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while seeding Enrollment data");
            throw;
        }
    }
}