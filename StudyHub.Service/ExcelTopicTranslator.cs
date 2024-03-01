using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

using OfficeOpenXml;
using OfficeOpenXml.Style;

using StudyHub.Common;
using StudyHub.Storage.Entities;

namespace StudyHub.Service;

public partial class ExcelTopicTranslator {

    [GeneratedRegex(@"^[a-zA-Z]+$")]
    private static partial Regex IsLetter();

    private readonly List<string> _subErrors = [];
    private readonly Dictionary<string, List<string>> _errors = [];

    protected record CellInfo(Topic Topic, string Value, int RowNumber, int ColumnNumber) { }

    public bool IsSuccess => _errors.Count == 0;

    public string GetErrors() {
        var builder = new StringBuilder();
        foreach (var keyValue in _errors) {
            foreach (var error in keyValue.Value) {
                builder.Append(keyValue.Key);
                builder.Append('：');
                builder.AppendLine(error);
            }
        }
        return builder.ToString();
    }

    public List<Topic> ParseExcelToTopics(Stream stream) {
        _errors.Clear();
        _subErrors.Clear();
        var topicAll = new List<Topic>();
        using var package = new ExcelPackage(stream);
        for (int i = 0; i < package.Workbook.Worksheets.Count; i++) {
            var worksheet = package.Workbook.Worksheets[i];
            var topics = ParseWorksheet(worksheet);
            if (_subErrors.Count > 0) {
                _errors.Add(worksheet.Name, [.. _subErrors]);
                _subErrors.Clear();
                continue;
            }
            topicAll.AddRange(topics);
        }
        return topicAll;
    }

    private List<Topic> ParseWorksheet(ExcelWorksheet worksheet) {
        string? message;
        var topics = new List<Topic>();
        for (int row = 2; row <= worksheet.Dimension.Rows; row++) {
            var topic = new Topic();
            for (int col = 1; col <= worksheet.Dimension.Columns; col++) {
                var cellValue = worksheet.Cells[row, col].Text.Trim();
                if (cellValue.Length > 2000) {
                    _subErrors.Add($"第{row}行{col}列的值长度不能大于2000");
                    continue;
                }
                var cellInfo = new CellInfo(topic, cellValue, row, col);
                switch (col) {
                    case 1:
                        if (TrySetTopicSubject(cellInfo, out message) is false) {
                            _subErrors.Add(message);
                        }
                        break;
                    case 2:
                        if (TrySetTopicText(cellInfo, out message) is false) {
                            _subErrors.Add(message);
                        }
                        break;
                    case 3:
                        if (TrySetTopicType(cellInfo, out message) is false) {
                            _subErrors.Add(message);
                        }
                        break;
                    case 4:
                        if (TrySetDifficultyLevel(cellInfo, out message) is false) {
                            _subErrors.Add(message);
                        }
                        break;
                    case 5:
                        if (TrySetCorrectAnswer(cellInfo, out message) is false) {
                            _subErrors.Add(message);
                        }
                        break;
                    case 6:
                        if (TrySetAnalysis(cellInfo, out message) is false) {
                            _subErrors.Add(message);
                        }
                        break;
                    default:
                        if (TryAddTopicOption(cellInfo, out message) is false) {
                            _subErrors.Add(message);
                        }
                        break;
                }
            }
            topics.Add(topic);
        }
        return topics;
    }

    private static bool TrySetTopicSubject(CellInfo cell, [NotNullWhen(false)] out string? error) {
        if (string.IsNullOrWhiteSpace(cell.Value)) {
            error = $"第{cell.RowNumber}行{cell.ColumnNumber}列的值不能为空白";
            return false;
        }
        cell.Topic.SubjectName = cell.Value;
        error = null;
        return true;
    }

    private static bool TrySetTopicText(CellInfo cell, [NotNullWhen(false)] out string? error) {
        if (string.IsNullOrWhiteSpace(cell.Value)) {
            error = $"第{cell.RowNumber}行{cell.ColumnNumber}列的值不能为空白";
            return false;
        }
        cell.Topic.TopicText = cell.Value;
        error = null;
        return true;
    }

    private static bool TrySetTopicType(CellInfo cell, [NotNullWhen(false)] out string? error) {
        if (string.IsNullOrWhiteSpace(cell.Value)) {
            error = $"第{cell.RowNumber}行{cell.ColumnNumber}列的值{cell.Value}不能为空白";
            return false;
        }
        var topicType = cell.Value switch {
            "单选题" => TopicType.Single,
            "多选题" => TopicType.Multiple,
            "判断题" => TopicType.TrueFalse,
            "填空题" => TopicType.Fill,
            _ => TopicType.None,
        };
        if (topicType == TopicType.None) {
            error = $"第{cell.RowNumber}行{cell.ColumnNumber}列的值{cell.Value}格式错误。正确值分别为：单选题、多选题、判断题、填空题";
            return false;
        }
        cell.Topic.TopicType = topicType;
        error = null;
        return true;
    }

    private static bool TrySetDifficultyLevel(CellInfo cell, [NotNullWhen(false)] out string? error) {
        if (string.IsNullOrWhiteSpace(cell.Value)) {
            error = $"第{cell.RowNumber}行{cell.ColumnNumber}列的值{cell.Value}不能为空白";
            return false;
        }
        var difficultyLevel = cell.Value switch {
            "初级" => DifficultyLevel.Easy,
            "中级" => DifficultyLevel.Medium,
            "高级" => DifficultyLevel.Hard,
            _ => DifficultyLevel.None,
        };
        if (difficultyLevel == DifficultyLevel.None) {
            error = $"第{cell.RowNumber}行{cell.ColumnNumber}列的值{cell.Value}格式错误。正确值分别为：初级、中级、高级";
            return false;
        }
        cell.Topic.DifficultyLevel = difficultyLevel;
        error = null;
        return true;
    }

    private static bool TrySetCorrectAnswer(CellInfo cell, [NotNullWhen(false)] out string? error) {
        if (string.IsNullOrWhiteSpace(cell.Value)) {
            // return $"第{row}行{col}列的值{cellValue}不能为空白";
            // 允许不设置答案（不设置答案的题目，提交任何答案都将判对）
            error = null;
            return true;
        }
        switch (cell.Topic.TopicType) {
            case TopicType.Single:
                if (!char.IsLetter(cell.Value, 0)) {
                    error = $"第{cell.RowNumber}行{cell.ColumnNumber}列的值{cell.Value}必须是字母";
                    return false;
                }
                cell.Topic.CorrectAnswer = cell.Value.ToUpper().First().ToString();
                break;
            case TopicType.Multiple:
                if (!IsLetter().IsMatch(cell.Value)) {
                    error = $"第{cell.RowNumber}行{cell.ColumnNumber}列的值{cell.Value}必须是字母";
                    return false;
                }
                cell.Topic.CorrectAnswer = cell.Value.ToUpper();
                break;
            case TopicType.TrueFalse:
                if (cell.Value != "0" && cell.Value != "1") {
                    error = $"第{cell.RowNumber}行{cell.ColumnNumber}列的值{cell.Value}必须是0或1";
                    return false;
                }
                cell.Topic.CorrectAnswer = cell.Value;
                break;
            case TopicType.Fill:
                cell.Topic.CorrectAnswer = cell.Value;
                break;
            default:
                break;
        }
        error = null;
        return true;
    }

    private static bool TrySetAnalysis(CellInfo cell, [NotNullWhen(false)] out string? error) {
        //if (string.IsNullOrWhiteSpace(cell.Value)) {
        //    error = $"第{cell.RowNumber}行{cell.ColumnNumber}列的值不能为空白";
        //    return false;
        //}
        cell.Topic.Analysis = cell.Value;
        error = null;
        return true;
    }

    private static bool TryAddTopicOption(CellInfo cell, [NotNullWhen(false)] out string? error) {
        if (cell.Topic.TopicType == TopicType.Single || cell.Topic.TopicType == TopicType.Multiple) {
            if (string.IsNullOrWhiteSpace(cell.Value)) {
                error = $"第{cell.RowNumber}行{cell.ColumnNumber}列的值{cell.Value}不能为空白";
                return false;
            }
            var c = cell.ColumnNumber - 6;// 减去前面的6列
            if (c > 26) {
                error = null;
                return true;
            }
            var option = new TopicOption {
                Text = cell.Value,
                Code = Convert.ToChar(c + 64),// A字母的ASCII是65，列索引从1开始
            };
            cell.Topic.TopicOptions.Add(option);
        }
        error = null;
        return true;
    }

    public static void TranslateTopicsToExcel(Stream stream, IList<Topic> topics) {
        using var package = new ExcelPackage();

        foreach (var item in topics.OrderBy(v => v.TopicType).ThenBy(v => v.DifficultyLevel).GroupBy(v => v.TopicType)) {
            var worksheet = package.Workbook.Worksheets.Add(item.Key.GetDescription());
            SetTitleToWorksheet(worksheet);
            SetTopicsToWorksheet(worksheet, [.. item]);
            SetWorksheetStyle(worksheet);
        }

        stream.Position = 0;
        package.SaveAs(stream);
        stream.Position = 0;
    }

    private static void SetTopicsToWorksheet(ExcelWorksheet worksheet, IList<Topic> topics) {
        for (int row = 2, index = 0; index < topics.Count; row++, index++) {
            var item = topics[index];
            worksheet.Cells[row, 1].Value = item.SubjectName;
            worksheet.Cells[row, 2].Value = item.TopicText;
            worksheet.Cells[row, 3].Value = GetTopicTypeString(item.TopicType);
            worksheet.Cells[row, 4].Value = GetDifficultyLevelString(item.DifficultyLevel);
            worksheet.Cells[row, 5].Value = item.TopicType is TopicType.TrueFalse ? int.TryParse(item.CorrectAnswer, out var result) ? result : item.CorrectAnswer : item.CorrectAnswer;
            worksheet.Cells[row, 6].Value = item.Analysis;
            int i = 1;
            foreach (var option in item.TopicOptions.OrderBy(v => v.Code)) {
                worksheet.Cells[row, 6 + i].Value = option.Text;
                i++;
            }
        }
    }

    private static void SetTitleToWorksheet(ExcelWorksheet worksheet) {
        worksheet.Cells[1, 1].Value = "科目";
        worksheet.Cells[1, 2].Value = "题目";
        worksheet.Cells[1, 3].Value = "题型";
        worksheet.Cells[1, 4].Value = "难度";
        worksheet.Cells[1, 5].Value = "答案";
        worksheet.Cells[1, 6].Value = "解析";
        worksheet.Cells[1, 7].Value = "选项A";
        worksheet.Cells[1, 8].Value = "选项B";
        worksheet.Cells[1, 9].Value = "选项C";
        worksheet.Cells[1, 10].Value = "选项D";
    }

    private static void SetWorksheetStyle(ExcelWorksheet worksheet) {
        using var allCellsRange = worksheet.Cells[worksheet.Dimension.Address];
        allCellsRange.Style.Font.Name = "微软雅黑";

        using var titleRange = worksheet.Cells["A1:J1"];
        titleRange.Style.Font.Bold = true;

        using var correctAnswerRange = worksheet.Cells[$"E1:E{worksheet.Dimension.End.Row}"];
        correctAnswerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    }

    private static string? GetTopicTypeString(TopicType topicType) {
        return topicType switch {
            TopicType.Single => "单选题",
            TopicType.Multiple => "多选题",
            TopicType.TrueFalse => "判断题",
            TopicType.Fill => "填空题",
            TopicType.None => null,
            _ => throw new NotImplementedException(),
        };
    }

    private static string GetDifficultyLevelString(DifficultyLevel level) {
        return level switch {
            DifficultyLevel.Easy => "初级",
            DifficultyLevel.Medium => "中级",
            DifficultyLevel.Hard => "高级",
            _ => "初级",
        };
    }
}
