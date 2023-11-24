namespace ALang.CodeGeneration;

internal sealed class Function(string name, FunctionParameter[] parameterTypes, LangType returnType)
{
	public readonly string Name = name;
	public readonly FunctionParameter[] Parameters = parameterTypes;
	public readonly LangType ReturnType = returnType;
	public readonly List<uint> Code = [];

	public bool TryGetParameterIndex(string name, out int index)
	{
		for (var i = 0; i < Parameters.Length; i++)
		{
			if (Parameters[i].Name == name)
			{
				index = i;
				return true;
			}
		}

		index = -1;
		return false;
	}
}
