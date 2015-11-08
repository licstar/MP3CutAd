exports.get = function(key) {
  try {
    return localStorage.getItem(key) || ''
  } catch (ex) {
    return ''
  }
}
exports.set = function(key, val) {
  try {
    localStorage.setItem(key, val)
  } catch (ex) {}
}

exports.getJson = function(key) {
  try {
    return JSON.parse(exports.get(key))
  } catch (ex) {
    return null
  }
}
exports.setJson = function(key, val) {
  try {
    exports.set(key, JSON.stringify(val))
  } catch (ex) {}
}