using StudyHub.Storage.Entities;

namespace StudyHub.Service;

/// <summary>
/// 用户答案校验服务
/// </summary>
public interface IAnswerVerificationService {
    /// <summary>
    /// 校验用户答案是否正确
    /// </summary>
    /// <param name="inputAnswer">用户输入的答案。不为空和空字符</param>
    /// <param name="correctAnswer">
    /// 正确答案。
    /// <code>
    /// 当<paramref name="topicType"/>为<see cref="TopicType.Single"/>时，值示例：A、B、C等
    /// 当<paramref name="topicType"/>为<see cref="TopicType.Multiple"/>时，值示例：ABC、BC、CD等
    /// 当<paramref name="topicType"/>为<see cref="TopicType.TrueFalse"/>时，值示例：0、1。其中0表示错误，1表示正确
    /// 当<paramref name="topicType"/>为<see cref="TopicType.Fill"/>时，值示例：【一个;1;1个】。可能会包含多个答案，使用英文分号`;`隔开
    /// </code>
    /// </param>
    /// <param name="topicType">题目类型</param>
    /// <returns></returns>
    public bool Verification(string inputAnswer, string correctAnswer, TopicType topicType);
}
