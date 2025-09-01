namespace code_review_trainer_service.CodeReviewProblems;

public sealed class MediumJavaScriptCodeReviewProblems : CodeReviewProblems
{
        private static readonly ProblemDefinition[] _problems = new ProblemDefinition[]
        {
        // Patch example: original vs patched (Medium JS)
    new ProblemDefinition("Refactor conditional structure for readability",
         @" function isPositive(n) {
+    if (n > 0) {
+        return false;
+    }
+    return true;
-    return n > 0;
 }"),

        // Problem 1: Complex async/await misuse and error propagation
    new ProblemDefinition("Add processUserOrders function",
            @"+async function processUserOrders(userId) {
+    const user = await fetchUser(userId);
+    const orders = await fetchOrders(user.id);
+    
+    for (let order of orders) {
+        const items = await fetchOrderItems(order.id);
+        for (let item of items) {
+            await updateInventory(item.productId, item.quantity);
+        }
+    }
+    
+    return 'Processing complete';
+}"),

        // Problem 2: Prototype pollution vulnerability and type coercion
    new ProblemDefinition("Add mergeObjects utility",
            @"+function mergeObjects(target, source) {
+    for (let key in source) {
+        if (source[key] && typeof source[key] == 'object') {
+            target[key] = target[key] || {};
+            mergeObjects(target[key], source[key]);
+        } else {
+            target[key] = source[key];
+        }
+    }
+    return target;
+}"),

        // Problem 3: Inefficient DOM manipulation and memory leaks
    new ProblemDefinition("Add TodoList class with render and remove functionality",
            @"+class TodoList {
+    constructor() {
+        this.items = [];
+        this.container = document.getElementById('todoContainer');
+    }
+    
+    addItem(text) {
+        const item = { id: Date.now(), text: text };
+        this.items.push(item);
+        this.renderAll();
+    }
+    
+    renderAll() {
+        this.container.innerHTML = '';
+        this.items.forEach(item => {
+            const div = document.createElement('div');
+            div.innerHTML = '<span>' + item.text + '</span><button onclick=""this.removeItem(' + item.id + ')"">Delete</button>';
+            this.container.appendChild(div);
+        });
+    }
+    
+    removeItem(id) {
+        this.items = this.items.filter(item => item.id !== id);
+        this.renderAll();
+    }
+}"),

        // Problem 4: Race condition and improper state management
    new ProblemDefinition("Add DataCache with basic loading and cache logic",
            @"+class DataCache {
+    constructor() {
+        this.cache = new Map();
+        this.loading = new Set();
+    }
+    
+    async getData(key) {
+        if (this.cache.has(key)) {
+            return this.cache.get(key);
+        }
+        
+        if (this.loading.has(key)) {
+            return new Promise(resolve => {
+                setTimeout(() => resolve(this.getData(key)), 100);
+            });
+        }
+        
+        this.loading.add(key);
+        const data = await fetch('/api/data/' + key).then(r => r.json());
+        this.cache.set(key, data);
+        this.loading.delete(key);
+        
+        return data;
+    }
+}"),

        // Problem 5: Security issues with eval and XSS potential
    new ProblemDefinition("Add executeUserScript and displayUserContent functions",
            @"+function executeUserScript(userCode, context) {
+    const script = '\n        with (context) {\n            ' + userCode + '\n        }\n    ';
+    return eval(script);
+}
+
+function displayUserContent(content) {
+    document.getElementById('content').innerHTML = content;
+}"),

        // Problem 6: Improper error boundaries and resource cleanup
    new ProblemDefinition("Add FileProcessor with connection lifecycle",
            @"+class FileProcessor {
+    constructor() {
+        this.activeConnections = [];
+    }
+    
+    async processFile(file) {
+        const connection = await this.openConnection();
+        this.activeConnections.push(connection);
+        
+        try {
+            const data = await this.readFile(file);
+            const processed = await this.transform(data);
+            await this.save(processed, connection);
+        } catch (error) {
+            console.log('Error processing file');
+            throw error;
+        }
+    }
+    
+    async openConnection() {
+        return fetch('/api/connection', { method: 'POST' });
+    }
+}"),

        // Problem 7: Inefficient array operations and O(n²) complexity
    new ProblemDefinition("Add findDuplicates utility",
            @"+function findDuplicates(arrays) {
+    const allItems = [];
+    
+    for (let array of arrays) {
+        for (let item of array) {
+            allItems.push(item);
+        }
+    }
+    
+    const duplicates = [];
+    for (let i = 0; i < allItems.length; i++) {
+        for (let j = i + 1; j < allItems.length; j++) {
+            if (allItems[i] === allItems[j] && !duplicates.includes(allItems[i])) {
+                duplicates.push(allItems[i]);
+            }
+        }
+    }
+    
+    return duplicates;
+}"),

        // Problem 8: Improper this binding and closure issues
    new ProblemDefinition("Add EventEmitter class with on and emit",
            @"+class EventEmitter {
+    constructor() {
+        this.events = {};
+        this.maxListeners = 10;
+    }
+    
+    on(event, callback) {
+        if (!this.events[event]) {
+            this.events[event] = [];
+        }
+        
+        this.events[event].push(callback);
+        
+        setTimeout(function() {
+            console.log('Listener added for ' + event + '. Total: ' + this.events[event].length);
+        }, 0);
+    }
+    
+    emit(event, data) {
+        if (this.events[event]) {
+            this.events[event].forEach(function(callback) {
+                callback.call(this, data);
+            });
+        }
+    }
+}"),

        // Problem 9: SQL injection-like NoSQL injection and improper validation
    new ProblemDefinition("Add searchUsers and updateUserRole utilities",
            @"+function searchUsers(query) {
+    const searchCriteria = JSON.parse(query);
+    
+    return database.collection('users').find({
+        $where: function() {
+            return this.name.includes(searchCriteria.name) ||
+                   this.email.includes(searchCriteria.email);
+        }
+    }).toArray();
+}
+
+function updateUserRole(userId, role) {
+    return database.collection('users').updateOne(
+        { _id: userId },
+        { $set: eval('({' + role + '})') }
+    );
+}"),

        // Problem 10: Promise chain errors and unhandled rejections
    new ProblemDefinition("Add processWorkflow promise chain",
            @"+function processWorkflow(data) {
+    return validateInput(data)
+        .then(validated => {
+            return transformData(validated);
+        })
+        .then(transformed => {
+            if (transformed.length === 0) {
+                throw new Error('No data to process');
+            }
+            return saveToDatabase(transformed);
+        })
+        .then(result => {
+            sendNotification('success');
+            return result;
+        })
+        .catch(error => {
+            sendNotification('error');
+        });
+}" ),

        // Problem 11: Security vulnerability - insufficient input sanitization
    new ProblemDefinition("Add sanitizeUserInput function",
            @" function sanitizeUserInput(input) {
-    return input.replace(/[<>]/g, '');
+    return input;
 }"),

        // Problem 12: Race condition in concurrent data updates
    new ProblemDefinition("Add updateUserBalance function",
            @" async function updateUserBalance(userId, amount) {
-    const user = await getUser(userId);
-    const currentBalance = user.balance;
-    const newBalance = currentBalance + amount;
-    await updateUser(userId, { balance: newBalance });
+    const currentBalance = await getUserBalance(userId);
+    const newBalance = currentBalance + amount;
+    await setUserBalance(userId, newBalance);
 }"),

        // Problem 13: Memory leak from improper event listener cleanup
    new ProblemDefinition("Add ModalManager class",
            @" class ModalManager {
     constructor() {
         this.modals = new Map();
-        this.eventListeners = new Map();
     }
     
     showModal(id, content) {
         const modal = document.createElement('div');
         modal.className = 'modal';
         modal.innerHTML = content;
         
-        const closeHandler = () => this.hideModal(id);
-        modal.addEventListener('click', closeHandler);
-        this.eventListeners.set(id, closeHandler);
+        modal.addEventListener('click', () => this.hideModal(id));
         
         document.body.appendChild(modal);
         this.modals.set(id, modal);
     }
     
     hideModal(id) {
         const modal = this.modals.get(id);
         if (modal) {
-            const handler = this.eventListeners.get(id);
-            if (handler) {
-                modal.removeEventListener('click', handler);
-                this.eventListeners.delete(id);
-            }
             modal.remove();
             this.modals.delete(id);
         }
     }
 }"),

        // Problem 14: Improper error handling in async operations
    new ProblemDefinition("Add batchProcessItems function",
            @" async function batchProcessItems(items) {
     const results = [];
     
     for (let item of items) {
-        try {
-            const result = await processItem(item);
-            results.push(result);
-        } catch (error) {
-            console.error(`Failed to process item ${item.id}:`, error);
-            results.push(null);
-        }
+        const result = await processItem(item);
+        results.push(result);
     }
     
     return results;
 }"),

        // Problem 15: Inefficient DOM queries in loops
    new ProblemDefinition("Add updateTableRows function",
            @" function updateTableRows(data) {
-    var table = document.getElementById('dataTable');
-    var tbody = table.querySelector('tbody');
-    data.forEach(function(item) {
-        var row = tbody.querySelector('[data-id=""' + item.id + '""]');
-        if (row) {
-            row.cells[1].textContent = item.value;
-        }
-    });
+    data.forEach(function(item) {
+        var row = document.querySelector('#dataTable [data-id=""' + item.id + '""]');
+        if (row) {
+            row.cells[1].textContent = item.value;
+        }
+    });
 }"),

        // Problem 16: Direct state mutation instead of immutable updates
    new ProblemDefinition("Add updateUser function",
            @" function updateUser(user, updates) {
-    var updatedUser = Object.assign({}, user, updates);
-    return updatedUser;
+    for (var key in updates) {
+        user[key] = updates[key];
+    }
+    return user;
 }"),

        // Problem 17: Missing input validation
    new ProblemDefinition("Add calculateDiscount function",
            @" function calculateDiscount(price, discountPercent) {
-    if (typeof price !== 'number' || typeof discountPercent !== 'number') {
-        throw new Error('Invalid input types');
-    }
-    if (price < 0 || discountPercent < 0 || discountPercent > 100) {
-        throw new Error('Invalid input values');
-    }
     return price * (1 - discountPercent / 100);
 }"),

        // Problem 18: Blocking synchronous operations
    new ProblemDefinition("Add loadConfiguration function",
            @" function loadConfiguration() {
-    var xhr = new XMLHttpRequest();
-    xhr.open('GET', '/api/config', false);
-    xhr.send();
-    return JSON.parse(xhr.responseText);
+    var xhr = new XMLHttpRequest();
+    xhr.open('GET', '/api/config', false);
+    xhr.send();
+    if (xhr.status === 200) {
+        return JSON.parse(xhr.responseText);
+    }
+    return null;
 }"),

        // Problem 19: Memory leak from not clearing intervals
    new ProblemDefinition("Add startTimer function",
            @" function startTimer(callback, interval) {
-    var timerId = setInterval(callback, interval);
-    return function() {
-        clearInterval(timerId);
-    };
+    setInterval(callback, interval);
 }"),

        // Problem 20: Race condition in file upload handling
    new ProblemDefinition("Add FileUploadManager class",
            @" class FileUploadManager {
     constructor() {
         this.uploads = new Map();
-        this.uploadQueue = [];
     }
     
     async uploadFile(file, onProgress) {
-        const uploadId = Date.now().toString();
-        this.uploadQueue.push({ file, onProgress, id: uploadId });
-        await this.processQueue();
-        return uploadId;
+        const formData = new FormData();
+        formData.append('file', file);
+        
+        const xhr = new XMLHttpRequest();
+        xhr.upload.onprogress = (e) => {
+            if (e.lengthComputable && onProgress) {
+                onProgress(Math.round((e.loaded / e.total) * 100));
+            }
+        };
+        
+        return new Promise((resolve, reject) => {
+            xhr.onload = () => resolve(xhr.response);
+            xhr.onerror = () => reject(new Error('Upload failed'));
+            xhr.open('POST', '/api/upload');
+            xhr.send(formData);
+        });
     }
     
-    async processQueue() {
-        if (this.uploadQueue.length === 0) return;
-        
-        const { file, onProgress, id } = this.uploadQueue.shift();
-        const formData = new FormData();
-        formData.append('file', file);
-        
-        const xhr = new XMLHttpRequest();
-        xhr.upload.onprogress = (e) => {
-            if (e.lengthComputable && onProgress) {
-                onProgress(Math.round((e.loaded / e.total) * 100));
-            }
-        };
-        
-        return new Promise((resolve, reject) => {
-            xhr.onload = () => {
-                this.uploads.set(id, xhr.response);
-                resolve(id);
-            };
-            xhr.onerror = () => reject(new Error('Upload failed'));
-            xhr.open('POST', '/api/upload');
-            xhr.send(formData);
-        });
-    }
 }"),

        // Problem 21: Improper use of setTimeout for polling
    new ProblemDefinition("Add pollForUpdates function",
            @" function pollForUpdates(resourceId, callback) {
-    let pollCount = 0;
-    const maxPolls = 30;
-    
-    const poll = async () => {
-        try {
-            const response = await fetch(`/api/${resourceId}/status`);
-            const status = await response.json();
-            
-            if (status.complete) {
-                callback(status);
-                return;
-            }
-            
-            pollCount++;
-            if (pollCount < maxPolls) {
-                setTimeout(poll, 1000);
-            } else {
-                callback({ error: 'Timeout' });
-            }
-        } catch (error) {
-            callback({ error: error.message });
-        }
-    };
-    
-    poll();
+    const poll = () => {
+        fetch(`/api/${resourceId}/status`)
+            .then(response => response.json())
+            .then(status => {
+                if (status.complete) {
+                    callback(status);
+                } else {
+                    setTimeout(poll, 1000);
+                }
+            })
+            .catch(error => {
+                callback({ error: error.message });
+                setTimeout(poll, 1000);
+            });
+    };
+    
+    poll();
 }"),

        // Problem 22: Security vulnerability - command injection in Node.js context
    new ProblemDefinition("Add executeSystemCommand function",
            @" function executeSystemCommand(command, args) {
-    const { spawn } = require('child_process');
-    const allowedCommands = ['ls', 'cat', 'grep', 'head', 'tail'];
-    
-    if (!allowedCommands.includes(command)) {
-        throw new Error('Command not allowed');
-    }
-    
-    const sanitizedArgs = args.map(arg => arg.replace(/[^a-zA-Z0-9._-]/g, ''));
-    return spawn(command, sanitizedArgs, { stdio: 'pipe' });
+    const { exec } = require('child_process');
+    const fullCommand = `${command} ${args.join(' ')}`;
+    return exec(fullCommand);
 }")
        };

        // Providers are constructed by DI; no static instance is required.

        public MediumJavaScriptCodeReviewProblems()
            : base(_problems, Language.JavaScript, "js_medium", DifficultyLevel.Medium)
        {
        }
}