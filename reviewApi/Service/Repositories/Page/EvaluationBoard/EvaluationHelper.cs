using reviewApi.DTO.Page.EvaluationBoard;
using reviewApi.Models;

namespace reviewApi.Service.Repositories.Page.EvaluationBoard
{
    public static class EvaluationHelper
    {
        public static List<CriteriaNodeDto> BuildCriteriaTree(
            IEnumerable<Criteria> criterias,
            Dictionary<string, EvaluationScore>? scoreMap = null,
            bool defaultSelfScore = false,
            string? parentCode = null)
        {
            return criterias
                .Where(c => c.VirtualParentCode == parentCode && c.IsActive == 1)
                .OrderBy(c => c.VirtualCode)
                .Select(c =>
                {
                    EvaluationScore? score = null;
                    scoreMap?.TryGetValue(c.VirtualCode, out score);
                    return new CriteriaNodeDto
                    {
                        VirtualCode = c.VirtualCode,
                        DisplayCode = c.DisplayCode,
                        Content = c.Content,
                        MaxScore = c.MaxScore,
                        ScoreType = c.ScoreType,
                        SelfScore = score?.SelfScore ?? (defaultSelfScore ? c.MaxScore : null),
                        ManagerScore = score?.ManagerScore,
                        Children = BuildCriteriaTree(criterias, scoreMap, defaultSelfScore, c.VirtualCode)
                    };
                })
                .ToList();
        }
    }
}
