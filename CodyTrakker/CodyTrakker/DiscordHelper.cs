using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CodyTrakker.CodyTrakker
{
    public class DiscordHelper
    {
        private readonly HttpClient _httpClient;
        private readonly string _webhookUrl;
        private readonly Dictionary<string, object> _payload;
        private readonly string username;
        private readonly string avatar_url;

        public DiscordHelper(string webhookUrl, string username = null, string avatarUrl = null)
        {
            _httpClient = new HttpClient();
            _webhookUrl = webhookUrl;
            _payload = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(username))
            {
                this.username = username;
                _payload["username"] = username;
            }

            if (!string.IsNullOrEmpty(avatarUrl))
            {
                avatar_url = avatarUrl;
                _payload["avatar_url"] = avatarUrl;
            }
        }

        public void AddMessage(string content)
        {
            if (_payload.ContainsKey("content"))
            {
                throw new InvalidOperationException("A message content is already added. Use a new DiscordHelper instance for another message.");
            }

            _payload["content"] = content;
        }

        public void AddEmbed(string title, string description, string color = null)
        {
            if (!_payload.ContainsKey("embeds"))
            {
                _payload["embeds"] = new List<Dictionary<string, object>>();
            }

            var embed = new Dictionary<string, object>
            {
                { "title", title },
                { "description", description }
            };

            if (!string.IsNullOrEmpty(color))
            {
                if (int.TryParse(color.Replace("#", ""), System.Globalization.NumberStyles.HexNumber, null, out int colorInt))
                {
                    embed["color"] = colorInt;
                }
                else
                {
                    throw new ArgumentException("Invalid color format. Use hex color format like #FFFFFF.");
                }
            }

            ((List<Dictionary<string, object>>)_payload["embeds"]).Add(embed);
        }

        public void AddField(string name, string value, bool inline = false)
        {
            if (!_payload.ContainsKey("embeds") || ((List<Dictionary<string, object>>)_payload["embeds"]).Count == 0)
            {
                throw new InvalidOperationException("No embed found to add a field. Add an embed first.");
            }

            var embeds = (List<Dictionary<string, object>>)_payload["embeds"];
            var lastEmbed = embeds[embeds.Count - 1];

            if (!lastEmbed.ContainsKey("fields"))
            {
                lastEmbed["fields"] = new List<Dictionary<string, object>>();
            }

            var field = new Dictionary<string, object>
            {
                { "name", name },
                { "value", value },
                { "inline", inline }
            };

            ((List<Dictionary<string, object>>)lastEmbed["fields"]).Add(field);
        }

        public async Task SendAsync()
        {
            if (_payload.Count == 0)
            {
                throw new InvalidOperationException("No content or embeds to send.");
            }

            var jsonPayload = JsonConvert.SerializeObject(_payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_webhookUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to send message. Status code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
            }
        }

        public void NewMessage()
        {
            _payload.Clear();
            _payload["username"] = username;
            _payload["avatar_url"] = avatar_url;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}