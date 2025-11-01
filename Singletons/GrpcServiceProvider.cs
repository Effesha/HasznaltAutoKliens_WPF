using Grpc.Net.Client;
using System;
using static HasznaltAuto.API.CarGrpc;
using static HasznaltAuto.API.HasznaltAutoGrpc;
using static HasznaltAuto.API.UserGrpc;

namespace HasznaltAuto.Desktop.Singletons
{
    public sealed class GrpcServiceProvider
    {
        private static readonly Lazy<GrpcServiceProvider> _instance = new(() => new GrpcServiceProvider());

        public static GrpcServiceProvider Instance => _instance.Value;

        private GrpcChannel Channel { get; }

        public HasznaltAutoGrpcClient HasznaltAutoGrpcClient { get; }
        public UserGrpcClient UserGrpcClient { get; }
        public CarGrpcClient CarGrpcClient { get; }

        private GrpcServiceProvider()
        {
            Channel = GrpcChannel.ForAddress("https://localhost:32767");
            HasznaltAutoGrpcClient = new HasznaltAutoGrpcClient(Channel);
            UserGrpcClient = new UserGrpcClient(Channel);
            CarGrpcClient = new CarGrpcClient(Channel);
        }
    }
}
