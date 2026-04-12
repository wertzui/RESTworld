const PROXY_CONFIG = [
    {
        context: [
            '/settings',
        ],
        target: process.env["services__ExampleBlog-Client__https__0"],
        secure: process.env["NODE_ENV"] !== "development"
    }
];

console.log(`Proxy configuration:\n${JSON.stringify(PROXY_CONFIG, null, 2)}`)

module.exports = PROXY_CONFIG;
