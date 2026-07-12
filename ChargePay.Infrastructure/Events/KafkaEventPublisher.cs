using ChargePay.Domain.Events;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ChargePay.Infrastructure.Events;

public class KafkaEventPublisher : IEventPublisher
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(IConfiguration configuration, ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;
        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            MessageTimeoutMs = 5000,
            SocketTimeoutMs = 3000,
            MessageSendMaxRetries = 1,
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task PublishAsync(DomainEvent domainEvent)
    {
        var topic = MapTopic(domainEvent);
        var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        try
        {
            await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = domainEvent.AggregateId.ToString(),
                Value = payload
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao publicar evento {EventType} no tópico {Topic}", domainEvent.GetType().Name, topic);
        }
    }

    private static string MapTopic(DomainEvent domainEvent) => domainEvent switch
    {
        UserCreatedDomainEvent => "identity.usuario-criado",
        UserAuthenticatedDomainEvent => "identity.usuario-autenticado",
        WalletCreatedDomainEvent => "wallet.carteira-criada",
        BalanceCreditedDomainEvent => "wallet.saldo-creditado",
        BalanceDebitedDomainEvent => "wallet.saldo-debitado",
        StationCreatedDomainEvent => "station.estacao-criada",
        ChargingSessionCreatedDomainEvent => "charging.sessao-criada",
        TelemetryReceivedDomainEvent => "telemetry.telemetria-recebida",
        EnergyConsumptionCalculatedDomainEvent => "tariff.consumo-calculado",
        ChargeGeneratedDomainEvent => "billing.cobranca-gerada",
        ChargingSessionCompletedDomainEvent => "charging.sessao-finalizada",
        ReceiptGeneratedDomainEvent => "billing.recibo-gerado",
        _ => "core-banking.unknown"
    };
}
