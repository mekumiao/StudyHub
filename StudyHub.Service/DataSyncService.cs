using System.Data.Common;

using Microsoft.EntityFrameworkCore;

using StudyHub.Common;
using StudyHub.Service;
using StudyHub.Service.Base;
using StudyHub.Storage.DbContexts;
using StudyHub.Storage.Entities;

namespace StudyHub.WPF.Services;

/// <summary>
/// 数据同步服务
/// </summary>
/// <param name="logger"></param>
/// <param name="serviceProvider"></param>
public class DataSyncService(
    ILogger<DataSyncService> logger,
    StudyHubDbContext dbContext,
    TopicService topicService,
    CourseService courseService,
    SettingService settingService) {
    private const string EvaluationBankSearchPattern = "*.bk1.xlsx";
    private const string SimulationBankSearchPattern = "*.bk2.xlsx";

    private async Task<ServiceResult> SyncCoursesAsync() {
        try {
            return await courseService.SynchronizeCourseToDbFromRootDirectoryAsync();
        }
        catch (DbException ex) {
            logger.LogError(ex, "同步课程时失败");
            return ServiceResult.Error(ex.Message);
        }
    }

    private async Task<bool> HasTopicsAsync() {
        return await dbContext.Topics.AnyAsync();
    }

    private async Task<bool> HasCoursesAsync() {
        return await dbContext.Courses.AnyAsync();
    }

    private async Task<bool> IsSyncCoursesAsync() {
        if (await settingService.GetOrAddStringValueAsync(SettingConstants.IsSyncCourses) is BooleanConstant.True) {
            return true;
        }
        if (await HasCoursesAsync()) {
            return true;
        }
        return false;
    }

    private async Task<bool> IsSyncTopicsAsync() {
        if (await settingService.GetOrAddStringValueAsync(SettingConstants.IsSyncTopics) is BooleanConstant.True) {
            return true;
        }
        if (await HasTopicsAsync()) {
            return true;
        }
        return false;
    }

    private async Task<ServiceResult> SyncTopicsFromStreamsAsync(IEnumerable<Stream> streams, TopicBankFlag topicBank) {
        try {
            logger.LogInformation("开始执行导入题目任务 目标题库：{topicBank}", topicBank.GetDescription());
            foreach (var item in streams) {
                await topicService.ImportTopicFromExcelStreamAsync(item, topicBank);
            }
        }
        catch (DbException ex) {
            logger.LogError(ex, "导入题目时发生错误");
            return ServiceResult.Error(ex.Message);
        }
        return ServiceResult.Ok();
    }

    public async Task SyncTopicsOnlyOnceAsync(Func<string, IEnumerable<Stream>> findStreams) {
        if (await IsSyncTopicsAsync()) return;
        try {
            await SyncTopicsFromStreamsAsync(findStreams.Invoke(EvaluationBankSearchPattern), TopicBankFlag.Evaluation);
            await SyncTopicsFromStreamsAsync(findStreams.Invoke(SimulationBankSearchPattern), TopicBankFlag.Simulation);
        }
        finally {
            await settingService.CreateOrUpdateAsync(SettingConstants.IsSyncTopics, BooleanConstant.True);
        }
    }

    public async Task SyncCouresOnlyOnceAsync() {
        if (await IsSyncCoursesAsync()) return;
        try {
            await SyncCoursesAsync();
        }
        finally {
            await settingService.CreateOrUpdateAsync(SettingConstants.IsSyncCourses, BooleanConstant.True);
        }
    }
}
