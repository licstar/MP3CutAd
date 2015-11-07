var _ = exports;

_.delimeter = navigator.platform.match(/win/i) ? '\\' : '/'

_.resolveFileName = function(path) {
  var arr = path.split(/[\/\\]/g)
  var filename = arr.pop()
  var directory = arr.join(_.delimeter)
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

_.extend = function(to, from) {
  var keys = Object.keys(from)
  var i = keys.length
  while (i--) {
    to[keys[i]] = from[keys[i]]
  }
  return to
}

_.floor = function(num) {
  return ~~num
}

_.rand = function(min, max) {
  if (max === undefined) {
    max = min
    min = 0
  }
  return _.floor(Math.random() * (max - min)) + min
}

_.padding = function(str, len, pad = ' ') {
  str = '' + str
  while (str.length < len) str = pad + str
  return str
}

_.qsa = function(selector, container = document) {
  return [].slice.call(container.querySelectorAll(selector), 0)
}

_.hsv2rgb = function(H, S, V) {
  var R, G, B
  if (S == 0) {
    R = G = B = V
  } else {
    H /= 60
    var i = _.floor(H)
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
  R = _.floor(R * 255)
  G = _.floor(G * 255)
  B = _.floor(B * 255)
  return [R, G, B]
}

_.rgb2hex = function(R, G, B) {
  return '#'
    + _.padding(R.toString(16), 2, '0')
    + _.padding(G.toString(16), 2, '0')
    + _.padding(B.toString(16), 2, '0')
}

_.formatDuration = function(time, showMs = false) {
  if (isNaN(time)) {
    return '--:--'
  }
  var ms = time % 1000
  time = _.floor(time / 1000)
  var s = time % 60
  time = _.floor(time / 60)
  var m = time % 60
  time = _.floor(time / 60)
  var h = time
  var text = ''
  if (h > 0) text += h + ':'
  if (!showMs || m > 0) text += (text ? _.padding(m, 2, '0') : m) + ':'
  text += (text ? _.padding(s, 2, '0') : s)
  if (showMs) text += ':' + _.padding(ms, 3, '0')

  return text
}

_.timestamp = function() {
  return (new Date).getTime()
}