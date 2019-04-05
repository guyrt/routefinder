using System.Collections.Generic;

namespace RouteCleaner.Model
{
    public abstract class TaggableIdentifiableElement
    {
        protected TaggableIdentifiableElement(string id, Dictionary<string, string> tags = null)
        {
            if (tags == null)
            {
                tags = new Dictionary<string, string>();
            }
            Tags = tags;
            Id = id;
        }

        public string Id { get; }

        public Dictionary<string, string> Tags { get; }
    }
}
