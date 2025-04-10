﻿using Core.Models.Entities;

namespace Core.DAL.Abstractions;

public interface IAssetsDal
{
    public Task<List<Assets>> GetAssetsAsync();
}