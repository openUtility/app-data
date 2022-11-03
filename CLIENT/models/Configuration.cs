namespace app_data_client.models {

    public class Configuration {

        public string Endpoint { get; set; }

        /// <summary>
        /// environment variable to pass onto the endpoint. 
        /// </summary> 
        public string Environment { get; set; }

        /// <summary>
        /// Amount of time to 'cache' results 
        /// </summary>
        public int CacheTimeoutInSeconds { get; set; } = 180;

        /// <summary>
        /// Switch names to 'preload' 
        /// </summary>
        public string[] Preload { get; set; } = null;

        /// <summary>
        /// placeholder, this is currently unused. 
        /// </summary>
        public string Token { get; set; }
    }
}