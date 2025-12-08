namespace SwapIndexer;

public static class IdGenerateHelper
{
    public static string GetId(params object[] inputs)
    {
        return inputs.JoinAsString("-");
    }
}