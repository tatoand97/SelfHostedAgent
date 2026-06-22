namespace SelfHostedAgent.Api.Services;

public sealed class BusinessContextService : IBusinessContextService
{
    public string GetBusinessContext()
    {
        return """
            Contoso Retail business context:
            - Horario de atencion: lunes a viernes de 8:00 a.m. a 6:00 p.m.
            - Devoluciones: hasta 30 dias con factura.
            - Pedidos: se consultan con numero de orden.
            - Soporte: chat, correo y linea telefonica.
            - Productos: la disponibilidad depende de ciudad y bodega.
            - Escalamiento: casos de pagos o fraude deben escalarse a soporte especializado.
            """;
    }
}
