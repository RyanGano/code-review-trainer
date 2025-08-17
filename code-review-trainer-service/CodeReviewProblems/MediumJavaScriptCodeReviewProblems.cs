namespace code_review_trainer_service.CodeReviewProblems;

// Static class containing medium-level JavaScript code review problems
public static class MediumJavaScriptCodeReviewProblems
{
    private static readonly Random _random = new Random();

    private static readonly string[] _problems = new string[]
    {
        // Problem 1: Complex async/await misuse and error propagation
        @"async function processUserOrders(userId) {
    const user = await fetchUser(userId);
    const orders = await fetchOrders(user.id);
    
    for (let order of orders) {
        const items = await fetchOrderItems(order.id);
        for (let item of items) {
            await updateInventory(item.productId, item.quantity);
        }
    }
    
    return 'Processing complete';
}",

        // Problem 2: Prototype pollution vulnerability and type coercion
        @"function mergeObjects(target, source) {
    for (let key in source) {
        if (source[key] && typeof source[key] == 'object') {
            target[key] = target[key] || {};
            mergeObjects(target[key], source[key]);
        } else {
            target[key] = source[key];
        }
    }
    return target;
}",

        // Problem 3: Inefficient DOM manipulation and memory leaks
        @"class TodoList {
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
}",

        // Problem 4: Race condition and improper state management
        @"class DataCache {
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
}",

        // Problem 5: Security issues with eval and XSS potential
        @"function executeUserScript(userCode, context) {
    const script = '\\n        with (context) {\\n            ' + userCode + '\\n        }\\n    ';
    return eval(script);
}

function displayUserContent(content) {
    document.getElementById('content').innerHTML = content;
}",

        // Problem 6: Improper error boundaries and resource cleanup
        @"class FileProcessor {
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
}",

        // Problem 7: Inefficient array operations and O(n²) complexity
        @"function findDuplicates(arrays) {
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
}",

        // Problem 8: Improper this binding and closure issues
        @"class EventEmitter {
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
}",

        // Problem 9: SQL injection-like NoSQL injection and improper validation
        @"function searchUsers(query) {
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
}",

        // Problem 10: Promise chain errors and unhandled rejections
        @"function processWorkflow(data) {
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
}"
    };

    public static string GetRandomProblem()
    {
        return _problems[_random.Next(_problems.Length)];
    }

    public static CodeReviewProblem GetRandomProblemWithId()
    {
        var index = _random.Next(_problems.Length);
        return new CodeReviewProblem
        {
            Id = $"js_medium_{index + 1:D3}",
            Problem = _problems[index],
            Language = Language.JavaScript
        };
    }

    public static int Count => _problems.Length;
    public static string GetProblemByIndex(int index) => _problems[index];
}