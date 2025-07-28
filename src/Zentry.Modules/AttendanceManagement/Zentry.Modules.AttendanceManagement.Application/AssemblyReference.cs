using System.Reflection;

namespace Zentry.Modules.AttendanceManagement.Application;

/// <summary>
/// Assembly reference for AttendanceManagement.Application module
/// Used by other modules and main application to get assembly reference
/// </summary>
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
} 