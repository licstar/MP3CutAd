exports.delimeter = navigator.platform.match(/win/i) ? '\\' : '/'

exports.resolveFileName = function(path) {
  var arr = path.split(/[\/\\]/g)
  var filename = arr.pop()
  var directory = arr.join(exports.delimeter)
  var dot = filename.indexOf('.')
  var basename = filename
  var extname = ''
  if (dot !== -1) {
    basename = filename.substring(0, dot)
    extname = filename.substring(dot)
  }

  return {
    fullname: path,
    filename,
    directory,
    basename,
    extname
  }
}

exports.extend = function(to, from) {
  var keys = Object.keys(from)
  var i = keys.length
  while (i--) {
    to[keys[i]] = from[keys[i]]
  }
  return to
}

exports.floor = function(num) {
  return ~~num
}

exports.rand = function(min, max) {
  if (max === undefined) {
    max = min
    min = 0
  }
  return exports.floor(Math.random() * (max - min)) + min
}

exports.padding = function(str, len, pad = ' ') {
  str = '' + str
  while (str.length < len) str = pad + str
  return str
}

exports.qsa = function(selector, container = document) {
  return [].slice.call(container.querySelectorAll(selector), 0)
}

exports.hsv2rgb = function(H, S, V) {
  var R, G, B
  if (S == 0) {
    R = G = B = V
  } else {
    H /= 60
    var i = exports.floor(H)
    var f = H - i
    var a = V * (1 - S)
    var b = V * (1 - S * f)
    var c = V * (1 - S * (1 - f))
    switch (i) {
      case 0:
        R = V
        G = c
        B = a
        break
      case 1:
        R = b
        G = V
        B = a
        break
      case 2:
        R = a
        G = V
        B = c
        break
      case 3:
        R = a
        G = b
        B = V
        break
      case 4:
        R = c
        G = a
        B = V
        break
      case 5:
        R = V
        G = a
        B = b
        break
    }
  }
  R = exports.floor(R * 255)
  G = exports.floor(G * 255)
  B = exports.floor(B * 255)
  return [R, G, B]
}

exports.rgb2hex = function(R, G, B) {
  return '#'
    + exports.padding(R.toString(16), 2, '0')
    + exports.padding(G.toString(16), 2, '0')
    + exports.padding(B.toString(16), 2, '0')
}