using Zentry.SharedKernel.Abstractions.Application;

namespace Zentry.SharedKernel.Contracts.Configuration;

public class GetUsersByStudentCodesIntegrationQuery : IQuery<GetUsersByStudentCodesIntegrationResponse>
{
    public List<string> StudentCodes { get; }

    public GetUsersByStudentCodesIntegrationQuery(List<string> studentCodes)
    {
        StudentCodes = studentCodes ?? throw new ArgumentNullException(nameof(studentCodes));
    }
}

public class GetUsersByStudentCodesIntegrationResponse
{
    public Dictionary<string, Guid> StudentCodeToUserIdMap { get; }

    public GetUsersByStudentCodesIntegrationResponse(Dictionary<string, Guid> studentCodeToUserIdMap)
    {
        StudentCodeToUserIdMap = studentCodeToUserIdMap;
    }
}
