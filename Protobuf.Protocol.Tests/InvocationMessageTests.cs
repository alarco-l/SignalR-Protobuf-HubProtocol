﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Protobuf.Protocol.Tests
{
    public class InvocationMessageTests
    {
        public const string TARGET = "Target";
        public const string INOVATION_ID = "123";

        [Theory]
        [InlineData("FooTarget")]
        [InlineData("InvocationMessageTarget")]
        [InlineData("TestInvocationMessageHubProtocolTarget")]
        public void Protocol_Should_Handle_InvocationMessage_Without_Argument(string target)
        {
            var logger = new NullLogger<ProtobufHubProtocol>();
            var binder = new Mock<IInvocationBinder>();

            var protobufHubProtocol = new ProtobufHubProtocol(logger);
            var writer = new ArrayBufferWriter<byte>();
            var invocationMessage = new InvocationMessage(target, Array.Empty<object>());

            protobufHubProtocol.WriteMessage(invocationMessage, writer);
            var encodedMessage = new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
            var result = protobufHubProtocol.TryParseMessage(ref encodedMessage, binder.Object, out var resultInvocationMessage);

            Assert.True(result);
            Assert.NotNull(resultInvocationMessage);
            Assert.IsType<InvocationMessage>(resultInvocationMessage);
            Assert.Equal(target, ((InvocationMessage)resultInvocationMessage).Target);
            Assert.Empty(((InvocationMessage)resultInvocationMessage).Arguments);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("1234")]
        [InlineData("9876543210123456789")]
        [InlineData("")]
        public void Protocol_Should_Handle_InvocationMessage_With_InvocationId_And_No_Argument(string invocationId)
        {
            var logger = new NullLogger<ProtobufHubProtocol>();
            var binder = new Mock<IInvocationBinder>();

            var protobufHubProtocol = new ProtobufHubProtocol(logger);
            var writer = new ArrayBufferWriter<byte>();
            var invocationMessage = new InvocationMessage(invocationId, TARGET, Array.Empty<object>());

            protobufHubProtocol.WriteMessage(invocationMessage, writer);
            var encodedMessage = new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
            var result = protobufHubProtocol.TryParseMessage(ref encodedMessage, binder.Object, out var resultInvocationMessage);

            Assert.True(result);
            Assert.NotNull(resultInvocationMessage);
            Assert.IsType<InvocationMessage>(resultInvocationMessage);
            Assert.Equal(TARGET, ((InvocationMessage)resultInvocationMessage).Target);
            Assert.Equal(invocationId, ((InvocationMessage)resultInvocationMessage).InvocationId);
            Assert.Empty(((InvocationMessage)resultInvocationMessage).Arguments);
        }

        [Theory]
        [InlineData("Single Argument")]
        [InlineData("Foo", "Bar")]
        [InlineData("### First Argument ###", "[Second] [Argument]", "%%% Third %%% Argument %%%", "$Forth-$-Argument$")]
        public void Protocol_Should_Handle_InvocationMessage_With_String_As_Argument(params string[] arguments)
        {
            var logger = new NullLogger<ProtobufHubProtocol>();
            var binder = new Mock<IInvocationBinder>();

            var protobufHubProtocol = new ProtobufHubProtocol(logger);
            var writer = new ArrayBufferWriter<byte>();
            var invocationMessage = new InvocationMessage(TARGET, arguments);

            protobufHubProtocol.WriteMessage(invocationMessage, writer);
            var encodedMessage = new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
            var result = protobufHubProtocol.TryParseMessage(ref encodedMessage, binder.Object, out var resultInvocationMessage);

            Assert.True(result);
            Assert.NotNull(resultInvocationMessage);
            Assert.IsType<InvocationMessage>(resultInvocationMessage);
            Assert.Equal(TARGET, ((InvocationMessage)resultInvocationMessage).Target);

            var args = ((InvocationMessage)resultInvocationMessage).Arguments;

            Assert.NotEmpty(args);
            Assert.Equal(arguments.Length, args.Length);
            
            for (var i = 0; i < args.Length; i++)
            {
                Assert.Equal(arguments[i], args[i]);
            }
        }
    }
}
