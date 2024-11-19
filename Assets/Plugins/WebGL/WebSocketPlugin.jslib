var WebSocketPlugin = {
    socket: null,

    WebSocketConnect: function(url) {
        var urlStr = UTF8ToString(url);
        this.socket = new WebSocket(urlStr);
        
        this.socket.onopen = function() {
            SendMessage('WebSocketHelper', 'OnOpen');
        };
        
        this.socket.onmessage = function(e) {
            SendMessage('WebSocketHelper', 'OnMessage', e.data);
        };
        
        this.socket.onerror = function(e) {
            SendMessage('WebSocketHelper', 'OnError', 'WebSocket error: ' + e.message);
        };
        
        this.socket.onclose = function(e) {
            SendMessage('WebSocketHelper', 'OnClose', 'WebSocket closed: ' + e.reason);
        };
    },

    WebSocketSend: function(message) {
        if (this.socket.readyState == 1) {
            this.socket.send(UTF8ToString(message));
        }
    },

    WebSocketClose: function() {
        if (this.socket != null) {
            this.socket.close();
        }
    }
};

mergeInto(LibraryManager.library, WebSocketPlugin);