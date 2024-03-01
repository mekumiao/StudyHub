using MapsterMapper;

using StudyHub.Service;
using StudyHub.Storage.Entities;
using StudyHub.WPF.Services;

namespace StudyHub.WPF.ViewModels.Pages;

public partial class SimulationTopicListViewModel(
    TopicSubjectOptionService topicSubjectOptionService,
    TopicService topicService,
    IMapper mapper,
    NotificationService notificationService) : TopicListViewModel(topicSubjectOptionService, topicService, mapper, notificationService) {
    protected override TopicBankFlag TopicBankFlag => TopicBankFlag.Simulation;
}
