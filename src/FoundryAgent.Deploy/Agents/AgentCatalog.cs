namespace FoundryAgent.Deploy.Agents;

public static class AgentCatalog
{
    public static IReadOnlyList<AgentSpecification> All { get; } = Array.AsReadOnly<AgentSpecification>(
    [
        new(
            Name: "support-agent",
            Instructions: """
                Eres un agente de soporte interno.
                Responde de forma clara, breve y precisa.
                Si no tienes informacion suficiente, indicalo directamente.
                No inventes politicas, datos internos ni procedimientos.
                Cuando la solicitud este fuera de alcance, recomienda escalarla a un humano.
                """)
    ]);
}
