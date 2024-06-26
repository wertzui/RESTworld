﻿namespace Aspire.Hosting.OtelCollector;

public class CollectorResource(string name) : ContainerResource(name)
{
    internal static string GRPCEndpointName = "grpc";
    internal static string HTTPEndpointName = "http";

    public EndpointReference GRPCEndpoint => new(this, GRPCEndpointName);
    public EndpointReference HTTPEndpoint => new(this, HTTPEndpointName);
}