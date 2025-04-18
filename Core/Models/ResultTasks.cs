﻿namespace Core.Models;

public class ResultTasks(bool isSuccess = true, string? errorMessage = null)
{
    public bool IsSuccess = isSuccess;
    public string? ErrorMessage = errorMessage;
    
    public void SetMessageError(string message)
    {
        ErrorMessage = message;
        IsSuccess = false;
    }

    public void RemoveAllMessages()
    {
        ErrorMessage = null;
        IsSuccess = true;
    }
}