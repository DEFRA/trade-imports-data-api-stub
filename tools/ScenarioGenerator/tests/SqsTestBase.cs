using System.Security.Cryptography;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Xunit.Abstractions;

namespace Defra.ScenarioGenerator.Tests;

public class SqsTestBase(ITestOutputHelper testOutputHelper)
{
    private const string QueueUrl =
        "http://sqs.eu-west-2.127.0.0.1:4566/000000000000/trade_imports_inbound_customs_declarations_processor.fifo"; // need to move queue name

    private readonly AmazonSQSClient _sqsClient = new(
        new BasicAWSCredentials("test", "test"),
        new AmazonSQSConfig
        {
            AuthenticationRegion = "eu-west-2",
            ServiceURL = "http://localhost:4566",
        }
    );

    protected Task<ReceiveMessageResponse> ReceiveMessage()
    {
        return _sqsClient.ReceiveMessageAsync(QueueUrl, CancellationToken.None);
    }

    protected Task<GetQueueAttributesResponse> GetQueueAttributes()
    {
        return _sqsClient.GetQueueAttributesAsync(
            new GetQueueAttributesRequest
            {
                AttributeNames = ["ApproximateNumberOfMessages"],
                QueueUrl = QueueUrl,
            },
            CancellationToken.None
        );
    }

    protected async Task SendMessage(
        string messageGroupId,
        string body,
        Dictionary<string, MessageAttributeValue>? messageAttributes = null
    )
    {
        var request = new SendMessageRequest
        {
            MessageAttributes = messageAttributes,
            MessageBody = body,
            MessageDeduplicationId = RandomNumberGenerator.GetString("abcdefg", 20),
            MessageGroupId = messageGroupId,
            QueueUrl = QueueUrl,
        };

        var result = await _sqsClient.SendMessageAsync(request, CancellationToken.None);

        testOutputHelper.WriteLine(
            "Sent {0} with groupID {1} to {2}",
            result.MessageId,
            messageGroupId,
            QueueUrl
        );
    }

    protected static Dictionary<string, MessageAttributeValue> WithInboundHmrcMessageType(
        string messageType
    )
    {
        return new Dictionary<string, MessageAttributeValue>
        {
            {
                "InboundHmrcMessageType",
                new MessageAttributeValue { DataType = "String", StringValue = messageType }
            },
        };
    }
}
