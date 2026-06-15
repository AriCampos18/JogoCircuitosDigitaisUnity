public static class LogicGateUtility
{
    public static bool Evaluate(LogicGateType gateType, bool firstInput, bool secondInput)
    {
        // Centraliza a tabela logica das portas para o puzzle e futuras expansoes.
        switch (gateType)
        {
            case LogicGateType.AND:
                return firstInput && secondInput;
            case LogicGateType.OR:
                return firstInput || secondInput;
            case LogicGateType.XOR:
                return firstInput != secondInput;
            case LogicGateType.NOT:
                return !firstInput;
            default:
                return false;
        }
    }

    public static string GetGateLabel(LogicGateType gateType)
    {
        switch (gateType)
        {
            case LogicGateType.AND:
                return "*";
            case LogicGateType.OR:
                return "+";
            case LogicGateType.XOR:
                return "(+)";
            case LogicGateType.NOT:
                return "!";
            default:
                return "?";
        }
    }
}
