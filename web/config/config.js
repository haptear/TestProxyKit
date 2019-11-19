export default {
    plugins: [
        ['umi-plugin-react', {
            dva: {
                immer: true
            },
            antd: true
        }]
    ],
    targets: { ie: 11 },
    outputPath: "./wwwroot",
    base: "/",
    publicPath: "/",
    cssPublicPath: "/",
    hash: true
}
