using EntityFramework.Exceptions.Common;

using Mapster;

using MapsterMapper;

using Microsoft.EntityFrameworkCore;

using StudyHub.Service.Base;
using StudyHub.Service.Models;
using StudyHub.Storage.DbContexts;
using StudyHub.Storage.Entities;

namespace StudyHub.Service;

internal class SettingServiceRegister : IRegister {
    public void Register(TypeAdapterConfig config) {
        config.NewConfig<SettingToken, Setting>()
            .Map(dest => dest.SettingType, src => src.Type)
            .Map(dest => dest.SettingName, src => src.Name)
            .Map(dest => dest.SettingValue, src => src.DefaultValue)
            .Map(dest => dest.SettingDescription, src => src.Description);
    }
}

public class SettingService(StudyHubDbContext dbContext, IMapper mapper) {
    public async Task<string> GetOrAddStringValueAsync(SettingToken token, Func<string>? valueFunc = null) {
        var setting = dbContext.Settings.SingleOrDefault(v => v.SettingType == token.Type && v.SettingName == token.Name);
        if (setting == null) {
            var res = await CreateAsync(token, valueFunc?.Invoke() ?? token.DefaultValue);
            if (res is not null) {
                return res.SettingValue;
            }
            return token.DefaultValue;
        }

        return setting.SettingValue;
    }

    public async Task<SettingDto?> CreateAsync(SettingToken token, string value) {
        var item = mapper.Map<Setting>(token);
        item.SettingValue = value;
        await dbContext.Settings.AddAsync(item);
        try {
            await dbContext.SaveChangesAsync();
        }
        catch (UniqueConstraintException) {
            item = await dbContext.Settings.SingleOrDefaultAsync(v => v.SettingName == token.Name && v.SettingType == token.Type);
            if (item is null) {
                return null;
            }
        }
        return mapper.Map<SettingDto>(item);
    }

    public async Task<bool> CreateOrUpdateAsync(SettingToken token, string value) {
        var item = await dbContext.Settings.SingleOrDefaultAsync(v => v.SettingType == token.Type && v.SettingName == token.Name);
        if (item is null) {
            item = mapper.Map<Setting>(token);
            item.SettingValue = value;
            await dbContext.Settings.AddAsync(item);
        }
        else {
            item.SettingValue = value;
        }
        return await dbContext.SaveChangesAsync() > 0;
    }

    public async Task<ServiceResult<SettingDto>> GetEntityByTokenAsync(SettingToken token) {
        var result = await dbContext.Settings.SingleOrDefaultAsync(v => v.SettingType == token.Type && v.SettingName == token.Name);
        if (result is null) return ServiceResult.NotFound<SettingDto>();
        return ServiceResult.Ok(mapper.Map<SettingDto>(result));
    }

    public async Task DeleteByTokenAsync(SettingToken token) {
        await dbContext.Settings.Where(v => v.SettingType == token.Type && v.SettingName == token.Name).ExecuteDeleteAsync();
    }
}
