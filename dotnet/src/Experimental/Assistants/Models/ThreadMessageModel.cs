﻿// Copyright (c) Microsoft. All rights reserved.
#pragma warning disable CA1812
#pragma warning disable CA1852

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Experimental.Assistants.Models;

/// <summary>
/// list of run steps belonging to a run.
/// </summary>
internal sealed class ThreadMessageListModel : OpenAIListModel<ThreadMessageModel>
{
    // No specialization
}

/// <summary>
/// Represents a message within a thread.
/// </summary>
internal sealed class ThreadMessageModel
{
    /// <summary>
    /// Identifier, which can be referenced in API endpoints.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Always "thread.message"
    /// </summary>
    [JsonPropertyName("object")]
#pragma warning disable CA1720 // Identifier contains type name - We don't control the schema
    public string Object { get; set; } = "thread.message";
#pragma warning restore CA1720 // Identifier contains type name

    /// <summary>
    /// Unix timestamp (in seconds) for when the message was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    /// <summary>
    /// The thread ID that this message belongs to.
    /// </summary>
    [JsonPropertyName("thread_id")]
    public string ThreadId { get; set; } = string.Empty;

    /// <summary>
    /// The entity that produced the message. One of "user" or "assistant".
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// The content of the message in array of text and/or images.
    /// </summary>
    [JsonPropertyName("content")]
    public List<ContentModel> Content { get; set; } = new List<ContentModel>();

    /// <summary>
    /// A list of file IDs that the assistant should use.
    /// </summary>
    [JsonPropertyName("file_ids")]
    public List<string> FileIds { get; set; } = new List<string>();

    /// <summary>
    /// If applicable, the ID of the assistant that authored this message.
    /// </summary>
    [JsonPropertyName("assistant_id")]
    public string AssistantId { get; set; } = string.Empty;

    /// <summary>
    /// If applicable, the ID of the run associated with the authoring of this message.
    /// </summary>
    [JsonPropertyName("run_id")]
    public string RunId { get; set; } = string.Empty;

    /// <summary>
    /// Set of 16 key-value pairs that can be attached to an object.
    /// This can be useful for storing additional information about the
    /// object in a structured format. Keys can be a maximum of 64
    /// characters long and values can be a maximum of 512 characters long.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Representa contents within a message.
    /// </summary>
    public sealed class ContentModel
    {
        /// <summary>
        /// Type of content.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Text context.
        /// </summary>
        [JsonPropertyName("text")]
        public TextContentModel? Text { get; set; }
    }

    /// <summary>
    /// Text content.
    /// </summary>
    public sealed class TextContentModel
    {
        /// <summary>
        /// The text itself.
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Any annotations on the text.
        /// </summary>
        [JsonPropertyName("annotations")]
        public List<object> Annotations { get; set; } = new List<object>();
    }
}
