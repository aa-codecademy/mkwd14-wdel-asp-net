namespace Lamazon.Domain.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string entityName, object id)
        : base($"{entityName} with id {id} was not found.")
    {
    }
}
