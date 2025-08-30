namespace code_review_trainer_service.CodeReviewProblems;

public sealed class MediumJavaScriptCodeReviewProblems : CodeReviewProblems
{
    private static readonly ProblemDefinition[] _problems = new ProblemDefinition[]
    {
        // Patch example: original vs patched (Medium JS)
        new ProblemDefinition(@"function isPositive(n) {
    return n > 0;
}", @"function isPositive(n) {
    if (n > 0) {
        return false;
    }
    return true;
}", "Refactor conditional structure for readability"),
        // Problem 1: Complex async/await misuse and error propagation
        new ProblemDefinition(string.Empty,@"async function processUserOrders(userId) {
    const user = await fetchUser(userId);
    const orders = await fetchOrders(user.id);
    
    for (let order of orders) {
        const items = await fetchOrderItems(order.id);
        for (let item of items) {
            await updateInventory(item.productId, item.quantity);
        }
    }
    
    return 'Processing complete';
}", "Add processUserOrders function"),

        // Problem 2: Prototype pollution vulnerability and type coercion
        new ProblemDefinition(string.Empty,@"function mergeObjects(target, source) {
    for (let key in source) {
        if (source[key] && typeof source[key] == 'object') {
            target[key] = target[key] || {};
            mergeObjects(target[key], source[key]);
        } else {
            target[key] = source[key];
        }
    }
    return target;
}", "Add mergeObjects utility"),

        // Problem 3: Inefficient DOM manipulation and memory leaks
        new ProblemDefinition(string.Empty,@"class TodoList {
    constructor() {
        this.items = [];
        this.container = document.getElementById('todoContainer');
    }
    
    addItem(text) {
        const item = { id: Date.now(), text: text };
        this.items.push(item);
        this.renderAll();
    }
    
    renderAll() {
        this.container.innerHTML = '';
        this.items.forEach(item => {
            const div = document.createElement('div');
            div.innerHTML = '<span>' + item.text + '</span><button onclick=""this.removeItem(' + item.id + ')"">Delete</button>';
            this.container.appendChild(div);
        });
    }
    
    removeItem(id) {
        this.items = this.items.filter(item => item.id !== id);
        this.renderAll();
    }
}", "Add TodoList class with render and remove functionality"),

        // Problem 4: Race condition and improper state management
        new ProblemDefinition(string.Empty,@"class DataCache {
    constructor() {
        this.cache = new Map();
        this.loading = new Set();
    }
    
    async getData(key) {
        if (this.cache.has(key)) {
            return this.cache.get(key);
        }
        
        if (this.loading.has(key)) {
            return new Promise(resolve => {
                setTimeout(() => resolve(this.getData(key)), 100);
            });
        }
        
        this.loading.add(key);
        const data = await fetch('/api/data/' + key).then(r => r.json());
        this.cache.set(key, data);
        this.loading.delete(key);
        
        return data;
    }
}", "Add DataCache with basic loading and cache logic"),

        // Problem 5: Security issues with eval and XSS potential
        new ProblemDefinition(string.Empty,@"function executeUserScript(userCode, context) {
    const script = '\n        with (context) {\n            ' + userCode + '\n        }\n    ';
    return eval(script);
}

function displayUserContent(content) {
    document.getElementById('content').innerHTML = content;
}", "Add executeUserScript and displayUserContent functions"),

        // Problem 6: Improper error boundaries and resource cleanup
        new ProblemDefinition(string.Empty,@"class FileProcessor {
    constructor() {
        this.activeConnections = [];
    }
    
    async processFile(file) {
        const connection = await this.openConnection();
        this.activeConnections.push(connection);
        
        try {
            const data = await this.readFile(file);
            const processed = await this.transform(data);
            await this.save(processed, connection);
        } catch (error) {
            console.log('Error processing file');
            throw error;
        }
    }
    
    async openConnection() {
        return fetch('/api/connection', { method: 'POST' });
    }
}", "Add FileProcessor with connection lifecycle"),

        // Problem 7: Inefficient array operations and O(n²) complexity
        new ProblemDefinition(string.Empty,@"function findDuplicates(arrays) {
    const allItems = [];
    
    for (let array of arrays) {
        for (let item of array) {
            allItems.push(item);
        }
    }
    
    const duplicates = [];
    for (let i = 0; i < allItems.length; i++) {
        for (let j = i + 1; j < allItems.length; j++) {
            if (allItems[i] === allItems[j] && !duplicates.includes(allItems[i])) {
                duplicates.push(allItems[i]);
            }
        }
    }
    
    return duplicates;
}", "Add findDuplicates utility"),

        // Problem 8: Improper this binding and closure issues
        new ProblemDefinition(string.Empty,@"class EventEmitter {
    constructor() {
        this.events = {};
        this.maxListeners = 10;
    }
    
    on(event, callback) {
        if (!this.events[event]) {
            this.events[event] = [];
        }
        
        this.events[event].push(callback);
        
        setTimeout(function() {
            console.log('Listener added for ' + event + '. Total: ' + this.events[event].length);
        }, 0);
    }
    
    emit(event, data) {
        if (this.events[event]) {
            this.events[event].forEach(function(callback) {
                callback.call(this, data);
            });
        }
    }
}", "Add EventEmitter class with on and emit"),

        // Problem 9: SQL injection-like NoSQL injection and improper validation
        new ProblemDefinition(string.Empty,@"function searchUsers(query) {
    const searchCriteria = JSON.parse(query);
    
    return database.collection('users').find({
        $where: function() {
            return this.name.includes(searchCriteria.name) ||
                   this.email.includes(searchCriteria.email);
        }
    }).toArray();
}

function updateUserRole(userId, role) {
    return database.collection('users').updateOne(
        { _id: userId },
        { $set: eval('({' + role + '})') }
    );
}", "Add searchUsers and updateUserRole utilities"),

        // Problem 10: Promise chain errors and unhandled rejections
        new ProblemDefinition(string.Empty,@"function processWorkflow(data) {
    return validateInput(data)
        .then(validated => {
            return transformData(validated);
        })
        .then(transformed => {
            if (transformed.length === 0) {
                throw new Error('No data to process');
            }
            return saveToDatabase(transformed);
        })
        .then(result => {
            sendNotification('success');
            return result;
        })
        .catch(error => {
            sendNotification('error');
        });
}", "Add processWorkflow promise chain")
    };

    // Providers are constructed by DI; no static instance is required.

    public MediumJavaScriptCodeReviewProblems()
        : base(_problems, Language.JavaScript, "js_medium", DifficultyLevel.Medium)
    {
    }
}