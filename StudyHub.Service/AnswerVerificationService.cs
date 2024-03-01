using StudyHub.Storage.Entities;

namespace StudyHub.Service;

public class AnswerVerificationService : IAnswerVerificationService {
    public bool Verification(string inputAnswer, string correctAnswer, TopicType topicType) {
        inputAnswer = inputAnswer.Trim();
        correctAnswer = correctAnswer.Trim();
        var result = correctAnswer.Equals(inputAnswer, StringComparison.CurrentCultureIgnoreCase);
        if (topicType is TopicType.Fill && result is false) {
            var corrects = correctAnswer.Split(';', StringSplitOptions.RemoveEmptyEntries);
            return corrects.Contains(inputAnswer);
        }
        return result;
    }
}
