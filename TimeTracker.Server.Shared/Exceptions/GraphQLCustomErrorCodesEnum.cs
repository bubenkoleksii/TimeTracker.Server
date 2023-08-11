namespace TimeTracker.Server.Shared.Exceptions
{
    public enum GraphQLCustomErrorCodesEnum
    {
        INVALID_PASSWORD,
        INVALID_INPUT_DATA,
        REFRESH_TOKEN_NOT_MATCHED,
        USER_ALREADY_EXISTS,
        USER_FIRED,
        INVALID_USER_STATUS,
        DATE_NULL,
        USER_HAS_PASSWORD,
        INVALID_WORK_SESSION_TYPE,
        WORK_SESSION_IS_ACTIVE,
        SEND_EMAIL_FAILED,
        OPERATION_FAILED,
        REFRESH_TOKEN_NOT_FOUND,
        USER_NOT_FOUND,
        WORK_SESSION_NOT_FOUND,
        HOLIDAY_NOT_FOUND,
        NO_PERMISSION,
        VACATION_NOT_FOUND
    }
}