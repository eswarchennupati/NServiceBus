﻿namespace NServiceBus
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Json;

    class SagaManifest
    {
        public string StorageDirectory { get; set; }
        public DataContractJsonSerializer Serializer { get; set; }
        public Type SagaEntityType { get; set; }

        public string GetFilePath(Guid sagaId)
        {
            return Path.Combine(StorageDirectory, sagaId + ".json");
        }
    }
}