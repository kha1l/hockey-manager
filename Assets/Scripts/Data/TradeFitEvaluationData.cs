using System;

[Serializable]
public class TradeFitEvaluationData
{
    public string EvaluatingTeamId;
    public string EvaluatingTeamName;
    public int IncomingValue;
    public int OutgoingValue;
    public int ValueDelta;
    public int NeedFitScore;
    public int CapFitScore;
    public int RosterFitScore;
    public int DirectionFitScore;
    public int ContractFitScore;
    public int FinalScore;
    public bool Accepted;
    public string Reason;
}
