namespace Core.DAL.Abstractions;

public struct ResultTaskDataBase(bool isSuccess = true, string? errorMessage = null)
{
    public bool IsSuccess = isSuccess;
    public string? ErrorMessage = errorMessage;
    
    

    public void SetMessageError(string message)
    {
        ErrorMessage = message;
        IsSuccess = false;
    }
}