namespace TimeTracker.Server.GraphQl.Auth.Types;

public class AuthInputType : InputObjectGraphType<UserRequest>
{
    public AuthInputType()
    {
        Name = "AuthInput";

    }
}