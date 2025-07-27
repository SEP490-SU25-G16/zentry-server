namespace Zentry.SharedKernel.Exceptions;

// Base business exception
public abstract class BusinessLogicException : Exception
{
    public BusinessLogicException(string message) : base(message)
    {
    } // Đổi từ protected thành public

    public BusinessLogicException(string message, Exception innerException) : base(message, innerException)
    {
    } // Đổi từ protected thành public
}

// User management exceptions - inherit directly from Exception for specific handling
public class UserNotFoundException : Exception
{
    public UserNotFoundException(Guid userId) : base($"User with ID '{userId}' not found")
    {
    }

    public UserNotFoundException(string message) : base(message)
    {
    }
}

public class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException(string identifier) : base($"User with identifier '{identifier}' already exists")
    {
    }

    public UserAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class AccountNotFoundException : Exception
{
    public AccountNotFoundException(Guid accountId) : base($"Account with ID '{accountId}' not found")
    {
    }

    public AccountNotFoundException(string message) : base(message)
    {
    }
}

// Resource exceptions
public class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string resourceName, object id) : base($"{resourceName} with ID '{id}' not found")
    {
    }

    public ResourceNotFoundException(string message) : base(message)
    {
    }
}

public class ResourceAlreadyExistsException : Exception
{
    public ResourceAlreadyExistsException(string resourceName, object identifier) : base(
        $"{resourceName} with identifier '{identifier}' already exists")
    {
    }

    public ResourceAlreadyExistsException(string message) : base(message)
    {
    }
}

// Schedule management exceptions
public class ScheduleConflictException : Exception
{
    public ScheduleConflictException(string message) : base(message)
    {
    }

    public ScheduleConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class ClassSectionNotFoundException : Exception
{
    public ClassSectionNotFoundException(Guid classSectionId) : base(
        $"Class section with ID '{classSectionId}' not found")
    {
    }

    public ClassSectionNotFoundException(string message) : base(message)
    {
    }
}

public class RoomNotAvailableException : Exception
{
    public RoomNotAvailableException(string roomId, DateTime dateTime) : base(
        $"Room '{roomId}' is not available at {dateTime}")
    {
    }

    public RoomNotAvailableException(string message) : base(message)
    {
    }
}

// Device management exceptions
public class DeviceAlreadyRegisteredException : Exception
{
    public DeviceAlreadyRegisteredException(string deviceId) : base(
        $"Device with ID '{deviceId}' is already registered")
    {
    }

    public DeviceAlreadyRegisteredException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

// Attendance exceptions
public class SessionNotFoundException : Exception
{
    public SessionNotFoundException(Guid sessionId) : base($"Session with ID '{sessionId}' not found")
    {
    }

    public SessionNotFoundException(string message) : base(message)
    {
    }
}

public class SessionAlreadyStartedException : Exception
{
    public SessionAlreadyStartedException(Guid sessionId) : base($"Session with ID '{sessionId}' has already started")
    {
    }

    public SessionAlreadyStartedException(string message) : base(message)
    {
    }
}

public class AttendanceCalculationFailedException : Exception
{
    public AttendanceCalculationFailedException(string message) : base(message)
    {
    }

    public AttendanceCalculationFailedException(string message, Exception innerException) : base(message,
        innerException)
    {
    }
}

// Configuration exceptions
public class ConfigurationException : Exception
{
    public ConfigurationException(string message) : base(message)
    {
    }

    public ConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class SettingNotFoundException : Exception
{
    public SettingNotFoundException(string settingName) : base($"Setting '{settingName}' not found")
    {
    }

    public SettingNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
