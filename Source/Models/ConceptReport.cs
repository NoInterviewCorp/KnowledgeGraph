namespace KnowledeGraph.Models
{
    public class ConceptReport
    {
        public string ConceptName { get; set; }
        public int KnowledgeIntensity { get; set; }
        public int ComprehensionIntensity { get; set; }
        public int ApplicationIntensity { get; set; }
        public int AnalysisIntensity { get; set; }
        public int SynthesisIntensity { get; set; }
        public int EvaluationIntensity { get; set; }
    }
}