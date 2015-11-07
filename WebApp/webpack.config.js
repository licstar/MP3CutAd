var webpack = require('webpack')
var path = require('path')

module.exports = {
  entry: './src/main.js',
  output: {
    path: './dist',
    publicPath: '/dist/',
    filename: 'build.js'
  },
  externals: {
    'vue': 'Vue'
  },
  module: {
    loaders: [
      {
        test: /\.js$/,
        include: [
          path.resolve(__dirname, 'src'),
        ],
        loader: 'babel'
      },
      {
        test: /\.vue$/,
        loader: 'vue'
      }
    ]
  },
  babel: {

  }
}

//if (process.env.NODE_ENV === 'production') {
if (process.argv.indexOf('--production') !== -1) {
  module.exports.plugins = [
    new webpack.DefinePlugin({
      'process.env': {
        NODE_ENV: '"production"'
      }
    }),
    new webpack.optimize.UglifyJsPlugin({
      compress: {
        warnings: false
      }
    }),
    new webpack.optimize.OccurenceOrderPlugin()
  ]
} else {
  module.exports.devtool = '#source-map'
}