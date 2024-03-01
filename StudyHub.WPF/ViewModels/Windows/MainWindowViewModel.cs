using System.Collections.ObjectModel;

using StudyHub.WPF.Helpers;
using StudyHub.WPF.Views.Pages;

using Wpf.Ui.Controls;

namespace StudyHub.WPF.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableObject {
    [ObservableProperty]
    private ObservableCollection<object> _menuItems = [
        new NavigationViewItem() {
            Content = "课程学习",
            Icon = GetIconFont("\ue638"),
            TargetPageType = typeof(CoursePage),
        },
        new NavigationViewItem() {
            Content = "考核系统",
            Icon = GetIconFont("\ue860"),
            TargetPageType = typeof(AssessmentPage),
        },
        new NavigationViewItem() {
            Content = "错题浏览",
            Icon = GetIconFont("\ue639"),
            TargetPageType = typeof(IncorrectlyTopicExplorerPage),
        },
        new NavigationViewItemSeparator(),
        new NavigationViewItem() {
            Content = "课程管理",
            Icon = GetIconFont("\ue60d"),
            MenuItemsSource = new[] {
                new NavigationViewItem() {
                    Content = "课程分类",
                    Icon = GetIconFont("\ue620"),
                    TargetPageType = typeof(CourseCategoriesPage),
                },
                new NavigationViewItem() {
                    Content = "课程列表",
                    Icon = GetIconFont("\ue6b2"),
                    TargetPageType = typeof(CourseListPage),
                },
            },
        },
        new NavigationViewItem() {
            Content = "题库管理",
            Icon = GetIconFont("\ue64c"),
            MenuItemsSource = new[] {
                new NavigationViewItem() {
                    Content = "科目列表",
                    Icon = GetIconFont("\ue641"),
                    TargetPageType = typeof(TopicSubjectListPage),
                },
                new NavigationViewItem() {
                    Content = "考核题目",
                    Icon = GetIconFont("\ue609"),
                    TargetPageType = typeof(TopicListPage),
                },
                new NavigationViewItem() {
                    Content = "模拟题目",
                    Icon = GetIconFont("\ue603"),
                    TargetPageType = typeof(SimulationTopicListPage),
                },
            },
        },
        new NavigationViewItem() {
            Content = "参数设置",
            Icon = GetIconFont("\ue610"),
            TargetPageType = typeof(ParameterSettingPage),
        },
    ];

    [ObservableProperty]
    private ObservableCollection<object> _footerMenuItems = [
        new NavigationViewItem() {
            Content = "设置",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
            TargetPageType = typeof(SettingsPage),
        }
    ];

    private static FontIcon GetIconFont(string glyph) {
        return new FontIcon {
            FontFamily = FontFamilyHelper.IconFont,
            Glyph = glyph
        };
    }
}
