namespace TimeTracker.Server.Shared.Exceptions
{
    public enum GraphQLCustomErrorCodesEnum
    {
        INVALID_PASSWORD,
        REFRESH_TOKEN_NOT_FOUND,
        REFRESH_TOKEN_NOT_MATCHED,
        USER_NOT_FOUND,
        USER_ALREADY_EXISTS,
        USER_FIRED,
        DATE_NULL,
        USER_HAS_PASSWORD,
        WORK_SESSION_NOT_FOUND,
        INVALID_WORK_SESSION_TYPE,
        WORK_SESSION_IS_ACTIVE,
        SEND_EMAIL_FAILED,
        OPERATION_FAILED,
        NO_PERMISSION
    }
}