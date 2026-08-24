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
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "The result is inverted", "The new body returns false when n is greater than zero and true otherwise. isPositive(5) is now false and isPositive(-5) is true, which is the opposite of the function's contract.", "critical", 3),
            new StoredReviewIssue("2", "Maintainability", "Four lines replace a single expression", "The commit claims a readability win, but return n > 0 was already the clearest possible form. if/return true/return false around a boolean is a well-known anti-pattern.", "medium", 2)
        ],
        @"function isPositive(n) {
    return n > 0;
}")),

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
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "No error handling around a multi-step mutation", "If updateInventory fails halfway through, the loop aborts with some inventory already written and some not. There is no try/catch, no compensation and no way for the caller to learn how far it got.", "high", 3),
            new StoredReviewIssue("2", "Performance", "Every request is awaited one at a time", "The nested loops serialise every call. For 50 orders of 10 items that is 500 sequential round trips where Promise.all over the independent work would overlap them.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "Returns a fixed string instead of a result", "'Processing complete' tells the caller nothing about what was processed or whether anything was skipped.", "low", 1)
        ],
        @"async function processUserOrders(userId) {
    const user = await fetchUser(userId);
    const orders = await fetchOrders(user.id);

    const results = await Promise.allSettled(
        orders.map(async (order) => {
            const items = await fetchOrderItems(order.id);
            await Promise.all(items.map((item) => updateInventory(item.productId, item.quantity)));
            return order.id;
        }),
    );

    return {
        processed: results.filter((r) => r.status === 'fulfilled').length,
        failed: results.filter((r) => r.status === 'rejected'),
    };
}")),

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
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Security", "Prototype pollution through __proto__ and constructor keys", "for...in walks attacker-controlled keys with no filtering, so merging {\"__proto__\": {\"isAdmin\": true}} writes onto Object.prototype and every object in the process inherits isAdmin. This is the classic prototype pollution sink.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "for...in also copies inherited properties", "Without a hasOwnProperty guard the loop picks up anything on the source's prototype chain, which is rarely what a merge is meant to do.", "medium", 2),
            new StoredReviewIssue("3", "Correctness", "Arrays and null are treated as plain objects", "typeof [] is 'object', so arrays are merged key by key rather than replaced or concatenated, producing surprising hybrids.", "medium", 2),
            new StoredReviewIssue("4", "Style", "Loose equality in the typeof check", "typeof always returns a string, so == works here, but === is what the rest of the codebase should use consistently.", "trivial", 1)
        ],
        @"function mergeObjects(target, source) {
    const forbidden = new Set(['__proto__', 'constructor', 'prototype']);

    for (const key of Object.keys(source)) {
        if (forbidden.has(key)) {
            continue;
        }

        const value = source[key];
        if (value && typeof value === 'object' && !Array.isArray(value)) {
            target[key] = mergeObjects(target[key] ?? {}, value);
        } else {
            target[key] = value;
        }
    }

    return target;
}")),

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
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Security", "Item text is interpolated into innerHTML", "Any todo containing markup is parsed as HTML, so a title of <img src=x onerror=alert(1)> executes. User text must go through textContent or be escaped.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "The delete button can never work", "Inside an inline onclick attribute this refers to the button element, not the TodoList instance, so this.removeItem is undefined and clicking Delete throws.", "high", 3),
            new StoredReviewIssue("3", "Performance", "The entire list is rebuilt on every change", "addItem and removeItem both call renderAll, which discards and recreates every node. Adding n items does O(n squared) DOM work and destroys focus and scroll position each time.", "medium", 2),
            new StoredReviewIssue("4", "Correctness", "Date.now() is not a reliable id", "Two items added in the same millisecond share an id, and removeItem then deletes both.", "low", 1)
        ],
        @"class TodoList {
    constructor(container = document.getElementById('todoContainer')) {
        this.items = [];
        this.container = container;
        this.container.addEventListener('click', (event) => {
            const id = event.target.closest('button')?.dataset.id;
            if (id) {
                this.removeItem(id);
            }
        });
    }

    addItem(text) {
        const item = { id: crypto.randomUUID(), text };
        this.items.push(item);
        this.container.appendChild(this.renderItem(item));
    }

    renderItem(item) {
        const div = document.createElement('div');
        const span = document.createElement('span');
        span.textContent = item.text;

        const button = document.createElement('button');
        button.textContent = 'Delete';
        button.dataset.id = item.id;

        div.append(span, button);
        div.dataset.id = item.id;
        return div;
    }

    removeItem(id) {
        this.items = this.items.filter((item) => item.id !== id);
        this.container.querySelector(`[data-id=""${id}""]`)?.remove();
    }
}")),

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
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "A failed fetch leaves the key marked as loading forever", "loading.delete only runs on the success path. After one rejection every later caller for that key falls into the polling branch and retries every 100ms indefinitely. The delete belongs in a finally.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "Waiting callers poll instead of sharing the in-flight promise", "The setTimeout loop wakes up every 100ms to re-check, which adds latency and can recurse deeply. Caching the promise itself lets every caller await the same request.", "medium", 2),
            new StoredReviewIssue("3", "Correctness", "HTTP errors are cached as data", "fetch resolves for 4xx and 5xx, so an error body is parsed and stored in the cache as though it were real data.", "medium", 2)
        ],
        @"class DataCache {
    constructor() {
        this.inFlight = new Map();
        this.cache = new Map();
    }

    async getData(key) {
        if (this.cache.has(key)) {
            return this.cache.get(key);
        }

        if (!this.inFlight.has(key)) {
            const promise = (async () => {
                const response = await fetch(`/api/data/${encodeURIComponent(key)}`);
                if (!response.ok) {
                    throw new Error(`Request failed: ${response.status}`);
                }
                return response.json();
            })().finally(() => this.inFlight.delete(key));

            this.inFlight.set(key, promise);
        }

        const data = await this.inFlight.get(key);
        this.cache.set(key, data);
        return data;
    }
}")),

        // Problem 5: Security issues with eval and XSS potential
    new ProblemDefinition("Add executeUserScript and displayUserContent functions",
            @"+function executeUserScript(userCode, context) {
+    const script = '\n        with (context) {\n            ' + userCode + '\n        }\n    ';
+    return eval(script);
+}
+
+function displayUserContent(content) {
+    document.getElementById('content').innerHTML = content;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Security", "User-supplied code is passed to eval", "executeUserScript runs arbitrary JavaScript with the full privileges of the page: it can read cookies, call any API as the logged-in user and exfiltrate anything on the origin. There is no sandbox and no way to make this safe by filtering.", "critical", 3),
            new StoredReviewIssue("2", "Security", "User content assigned to innerHTML", "displayUserContent parses whatever it is given as HTML, so any script tag, event handler attribute or javascript: URL in the content executes. textContent, or a sanitizer, is required.", "critical", 3),
            new StoredReviewIssue("3", "Maintainability", "with is forbidden in strict mode", "Beyond the security problem, the with statement is a syntax error in strict mode and in modules, so this cannot be used from modern code at all.", "medium", 2)
        ],
        @"function displayUserContent(content) {
    document.getElementById('content').textContent = content;
}

// Arbitrary user code cannot be executed safely in the page context.
// Run it in a sandboxed worker or on the server with an explicit allow-list of operations.")),

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
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Connections are never released", "Every call pushes onto activeConnections and nothing ever removes or closes an entry, on the success path or the failure path. The array grows for the lifetime of the process and the remote side runs out of connections. A finally block is what the try is missing.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "The catch discards the error it logs", "console.log('Error processing file') prints a constant string with no file name and no error detail, so the log tells an operator nothing. The catch adds nothing over letting the error propagate.", "medium", 2),
            new StoredReviewIssue("3", "Correctness", "openConnection returns a fetch Response", "The value pushed into activeConnections is an HTTP response, not a connection handle, and its ok status is never checked.", "medium", 2)
        ],
        @"class FileProcessor {
    constructor() {
        this.activeConnections = new Set();
    }

    async processFile(file) {
        const connection = await this.openConnection();
        this.activeConnections.add(connection);

        try {
            const data = await this.readFile(file);
            const processed = await this.transform(data);
            return await this.save(processed, connection);
        } catch (error) {
            console.error(`Failed to process ${file.name}`, error);
            throw error;
        } finally {
            this.activeConnections.delete(connection);
            await connection.close();
        }
    }
}")),

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
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Performance", "Cubic behaviour on the duplicate scan", "The nested loops are already O(n squared), and duplicates.includes inside the inner loop adds another linear scan, so the worst case is O(n cubed). Two Sets answer the same question in a single pass.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "Manual flattening loop", "The first double loop is arrays.flat(). Writing it out adds five lines and no clarity.", "low", 1)
        ],
        @"function findDuplicates(arrays) {
    const seen = new Set();
    const duplicates = new Set();

    for (const item of arrays.flat()) {
        if (seen.has(item)) {
            duplicates.add(item);
        } else {
            seen.add(item);
        }
    }

    return [...duplicates];
}")),

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
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "this is not the emitter inside the setTimeout callback", "A function expression gets its own this. In a class body that means undefined, so this.events throws TypeError on the next tick after every on() call. An arrow function keeps the lexical this.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "callback.call(this, data) passes the wrong receiver", "The forEach callback is also a function expression, so this is undefined there too and every listener is invoked with an undefined receiver. Passing the emitter, or just calling callback(data), is what was meant.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "maxListeners is stored and never used", "The field suggests a leak guard that does not exist. Either enforce it in on() or drop it.", "low", 1),
            new StoredReviewIssue("4", "Maintainability", "There is no way to remove a listener", "Without an off method, subscribers are retained for the lifetime of the emitter, which is the usual source of listener leaks.", "medium", 2)
        ],
        @"class EventEmitter {
    constructor(maxListeners = 10) {
        this.events = new Map();
        this.maxListeners = maxListeners;
    }

    on(event, callback) {
        const listeners = this.events.get(event) ?? [];
        if (listeners.length >= this.maxListeners) {
            console.warn(`Possible listener leak on ""${event}""`);
        }

        listeners.push(callback);
        this.events.set(event, listeners);
        return () => this.off(event, callback);
    }

    off(event, callback) {
        const listeners = this.events.get(event);
        if (listeners) {
            this.events.set(event, listeners.filter((listener) => listener !== callback));
        }
    }

    emit(event, data) {
        for (const listener of this.events.get(event) ?? []) {
            listener(data);
        }
    }
}")),

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
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Security", "eval on the role parameter is remote code execution", "eval('({' + role + '})') runs whatever the caller sent inside the application process. A role of \"a:1}); require('child_process').exec('curl attacker'); ({\" executes commands on the server.", "critical", 3),
            new StoredReviewIssue("2", "Security", "$where executes JavaScript inside the database", "$where runs server-side JS per document, which is both a NoSQL injection vector and a full collection scan. A normal query with $regex or an indexed field lookup does the same job safely.", "critical", 3),
            new StoredReviewIssue("3", "Correctness", "JSON.parse is unguarded", "Malformed input throws a SyntaxError out of searchUsers, and a criteria object without name or email makes includes() throw inside the database.", "medium", 2)
        ],
        @"function searchUsers(criteria) {
    const name = String(criteria?.name ?? '');
    const email = String(criteria?.email ?? '');

    return database.collection('users').find({
        $or: [
            { name: { $regex: escapeRegex(name), $options: 'i' } },
            { email: { $regex: escapeRegex(email), $options: 'i' } },
        ],
    }).toArray();
}

const ALLOWED_ROLES = new Set(['admin', 'editor', 'viewer']);

function updateUserRole(userId, role) {
    if (!ALLOWED_ROLES.has(role)) {
        throw new Error(`Unknown role: ${role}`);
    }

    return database.collection('users').updateOne({ _id: userId }, { $set: { role } });
}")),

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
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "The catch swallows the error and resolves with undefined", "Because the handler returns nothing, the chain succeeds with undefined after a failure. Callers cannot distinguish a saved workflow from a failed one, and the error object is discarded entirely.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "The failure is never logged", "sendNotification('error') is the only trace. Nothing records which step failed or why, so this is undiagnosable in production.", "medium", 2),
            new StoredReviewIssue("3", "Correctness", "Notification failures are unhandled", "If sendNotification itself rejects inside the catch, that rejection escapes with nothing to handle it.", "low", 1)
        ],
        @"async function processWorkflow(data) {
    try {
        const validated = await validateInput(data);
        const transformed = await transformData(validated);
        if (transformed.length === 0) {
            throw new Error('No data to process');
        }

        const result = await saveToDatabase(transformed);
        await sendNotification('success');
        return result;
    } catch (error) {
        console.error('Workflow failed', error);
        await sendNotification('error');
        throw error;
    }
}")),

        // Problem 11: Security vulnerability - insufficient input sanitization
    new ProblemDefinition("Add sanitizeUserInput function",
            @" function sanitizeUserInput(input) {
-    return input.replace(/[<>]/g, '');
+    return input;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Security", "The sanitizer no longer sanitizes anything", "sanitizeUserInput now returns its argument unchanged, so every call site that trusts it to strip markup is exposed. The function name will keep reviewers of other files from noticing.", "critical", 3),
            new StoredReviewIssue("2", "Security", "Stripping angle brackets was never sufficient", "Even the removed version did not handle quotes, javascript: URLs or attribute contexts. The right fix is contextual escaping or a maintained sanitizer library, not a character blacklist.", "medium", 2)
        ],
        @"import DOMPurify from 'dompurify';

function sanitizeUserInput(input) {
    return DOMPurify.sanitize(String(input));
}")),

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
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Read-modify-write on money with no atomicity", "Two concurrent calls both read the same starting balance and the second write overwrites the first, so one deposit disappears. The rename in this patch does not address it - the update needs to be a single atomic increment or run inside a transaction with the row locked.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "No validation or failure handling", "amount is not checked, so NaN or a negative value writes a corrupt balance, and a failure in setUserBalance leaves the caller with no indication.", "medium", 2)
        ],
        @"async function updateUserBalance(userId, amount) {
    if (!Number.isFinite(amount)) {
        throw new TypeError('amount must be a finite number');
    }

    // Single atomic statement: no read-modify-write window.
    return db.query(
        'UPDATE users SET balance = balance + $1 WHERE id = $2 RETURNING balance',
        [amount, userId],
    );
}")),

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
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Maintainability", "The handler reference needed for cleanup was discarded", "The inline arrow function cannot be passed to removeEventListener, so the listener can never be detached explicitly. It also closes over the manager, keeping it reachable from the DOM node for as long as that node lives.", "medium", 2),
            new StoredReviewIssue("2", "Security", "Modal content is assigned to innerHTML", "showModal parses caller-supplied content as HTML. If any of it comes from user data this is a stored XSS sink, and this patch leaves it in place while removing the surrounding bookkeeping.", "high", 3),
            new StoredReviewIssue("3", "Correctness", "Clicking anywhere in the modal closes it", "The listener is on the modal itself, so a click on its content bubbles up and dismisses it. The old code had the same problem, but the patch was the moment to notice it.", "low", 1)
        ],
        @"class ModalManager {
    constructor() {
        this.modals = new Map();
    }

    showModal(id, content) {
        const modal = document.createElement('div');
        modal.className = 'modal';
        modal.textContent = content;

        const controller = new AbortController();
        modal.addEventListener('click', (event) => {
            if (event.target === modal) {
                this.hideModal(id);
            }
        }, { signal: controller.signal });

        document.body.appendChild(modal);
        this.modals.set(id, { modal, controller });
    }

    hideModal(id) {
        const entry = this.modals.get(id);
        if (entry) {
            entry.controller.abort();
            entry.modal.remove();
            this.modals.delete(id);
        }
    }
}")),

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
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "One bad item now aborts the whole batch", "Without the try/catch the first rejection propagates out of the loop, discarding the results already collected and leaving the remaining items unprocessed. For a batch operation that is almost never the desired behaviour.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "The per-item error log was removed", "The old catch named the failing item id. Now an operator sees a single stack trace with no indication of which item caused it.", "medium", 2),
            new StoredReviewIssue("3", "Performance", "Items are processed strictly one at a time", "If processItem is I/O bound, Promise.allSettled over the batch would overlap the work and also give per-item success and failure for free.", "low", 1)
        ],
        @"async function batchProcessItems(items) {
    const settled = await Promise.allSettled(items.map((item) => processItem(item)));

    return settled.map((outcome, index) => {
        if (outcome.status === 'fulfilled') {
            return outcome.value;
        }

        console.error(`Failed to process item ${items[index].id}:`, outcome.reason);
        return null;
    });
}")),

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
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Performance", "Each row is searched for from the document root", "The patch drops the cached tbody, so every iteration re-resolves #dataTable and then scans its descendants. For a thousand rows that is a thousand document-wide selector matches instead of a thousand lookups inside a known subtree.", "medium", 2),
            new StoredReviewIssue("2", "Correctness", "Selector is built by string concatenation", "An id containing a quote or a space produces an invalid selector and throws. CSS.escape, or a Map built from the rows once, avoids that.", "low", 1),
            new StoredReviewIssue("3", "Maintainability", "var in new code", "The function was already using var, but the rewritten block was the opportunity to move to const.", "trivial", 1)
        ],
        @"function updateTableRows(data) {
    const tbody = document.querySelector('#dataTable tbody');
    if (!tbody) {
        return;
    }

    const rowsById = new Map(
        [...tbody.querySelectorAll('[data-id]')].map((row) => [row.dataset.id, row]),
    );

    for (const item of data) {
        const row = rowsById.get(String(item.id));
        if (row) {
            row.cells[1].textContent = item.value;
        }
    }
}")),

        // Problem 16: Direct state mutation instead of immutable updates
    new ProblemDefinition("Add updateUser function",
            @" function updateUser(user, updates) {
-    var updatedUser = Object.assign({}, user, updates);
-    return updatedUser;
+    for (var key in updates) {
+        user[key] = updates[key];
+    }
+    return user;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "The caller's user object is now mutated", "The old version copied into a fresh object. The new one writes into the argument, so anything else holding that reference sees the change. In a framework that compares by identity the re-render is also skipped, because the returned object is the same one.", "medium", 2),
            new StoredReviewIssue("2", "Security", "for...in over caller-supplied keys", "The loop copies inherited properties and does not filter __proto__, so a crafted updates object can reach the prototype chain.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "var in new code", "The loop variable should be const, and the function no longer needs a temporary at all.", "trivial", 1)
        ],
        @"function updateUser(user, updates) {
    return { ...user, ...updates };
}")),

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
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Range validation removed from a pricing calculation", "A discountPercent of 150 now returns a negative price and a negative price is accepted outright. Both flow into whatever charges the customer, and the removed guard was the only thing stopping them.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "Type validation removed", "calculateDiscount('100', undefined) returns NaN instead of throwing, so the failure surfaces somewhere far from the cause.", "medium", 2)
        ],
        @"function calculateDiscount(price, discountPercent) {
    if (typeof price !== 'number' || typeof discountPercent !== 'number') {
        throw new TypeError('Invalid input types');
    }
    if (price < 0 || discountPercent < 0 || discountPercent > 100) {
        throw new RangeError('Invalid input values');
    }

    return price * (1 - discountPercent / 100);
}")),

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
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Performance", "Synchronous XHR freezes the main thread", "The false argument to open makes the request blocking, so the page cannot paint or respond to input until the server answers. Browsers have deprecated this and log a warning; the patch keeps it and only adds a status check.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "Failures are reported as null", "Returning null for any non-200 means the caller cannot tell a 404 from a 500 from a config that legitimately has no content, and nothing is logged.", "medium", 2),
            new StoredReviewIssue("3", "Correctness", "JSON.parse is still unguarded", "A 200 response with a non-JSON body throws a SyntaxError out of a function that otherwise reports failure by returning null.", "low", 1)
        ],
        @"async function loadConfiguration() {
    const response = await fetch('/api/config');
    if (!response.ok) {
        throw new Error(`Failed to load configuration: ${response.status}`);
    }

    return response.json();
}")),

        // Problem 19: Memory leak from not clearing intervals
    new ProblemDefinition("Add startTimer function",
            @" function startTimer(callback, interval) {
-    var timerId = setInterval(callback, interval);
-    return function() {
-        clearInterval(timerId);
-    };
+    setInterval(callback, interval);
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "The timer can never be stopped", "The interval id is discarded and nothing is returned, so there is no way to call clearInterval. The callback and everything it closes over stay alive for the lifetime of the page, and each call to startTimer adds another one that runs forever.", "high", 3)
        ],
        @"function startTimer(callback, interval) {
    const timerId = setInterval(callback, interval);
    return () => clearInterval(timerId);
}")),

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
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Removing the queue lets uploads run unbounded and concurrent", "Every call now starts an XHR immediately, so a user selecting fifty files opens fifty simultaneous uploads. The serialisation the queue provided is gone and nothing replaces it.", "medium", 2),
            new StoredReviewIssue("2", "Correctness", "this.uploads is never written to again", "The map is still created in the constructor but only processQueue populated it. Any code that looked up a completed upload by id now finds nothing.", "medium", 2),
            new StoredReviewIssue("3", "Correctness", "onload resolves for HTTP error responses", "XHR fires onload for a 500 as readily as a 200. Without checking xhr.status a failed upload resolves successfully with an error body.", "medium", 2),
            new StoredReviewIssue("4", "Maintainability", "There is no way to cancel an upload", "Neither the old nor the new code keeps the xhr around, so a user cannot abort a large upload.", "low", 1)
        ],
        @"class FileUploadManager {
    constructor(maxConcurrent = 3) {
        this.uploads = new Map();
        this.active = 0;
        this.maxConcurrent = maxConcurrent;
        this.queue = [];
    }

    uploadFile(file, onProgress) {
        return new Promise((resolve, reject) => {
            this.queue.push({ file, onProgress, resolve, reject });
            this.drain();
        });
    }

    drain() {
        while (this.active < this.maxConcurrent && this.queue.length > 0) {
            const job = this.queue.shift();
            this.active++;
            this.send(job).finally(() => {
                this.active--;
                this.drain();
            });
        }
    }

    async send({ file, onProgress, resolve, reject }) {
        const id = crypto.randomUUID();
        const formData = new FormData();
        formData.append('file', file);

        const xhr = new XMLHttpRequest();
        this.uploads.set(id, xhr);

        xhr.upload.onprogress = (e) => {
            if (e.lengthComputable && onProgress) {
                onProgress(Math.round((e.loaded / e.total) * 100));
            }
        };

        return new Promise((done) => {
            xhr.onload = () => {
                if (xhr.status >= 200 && xhr.status < 300) {
                    resolve(id);
                } else {
                    reject(new Error(`Upload failed: ${xhr.status}`));
                }
                done();
            };
            xhr.onerror = () => {
                reject(new Error('Upload failed'));
                done();
            };
            xhr.open('POST', '/api/upload');
            xhr.send(formData);
        });
    }
}")),

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
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "The attempt limit was removed", "maxPolls used to stop after 30 tries and report a timeout. The new loop polls forever, so a resource that never completes keeps hitting the server for the lifetime of the page.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "Errors invoke the callback and then keep polling", "The catch calls callback with an error and immediately schedules another attempt, so the caller receives an unbounded stream of error callbacks for a permanently failing resource.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "There is no way to cancel the poll", "Nothing is returned, so a caller that navigates away cannot stop the timer. A fixed 1000ms interval with no backoff also hammers a struggling server.", "medium", 2)
        ],
        @"function pollForUpdates(resourceId, callback, { maxPolls = 30, interval = 1000 } = {}) {
    let attempts = 0;
    let timer;

    const poll = async () => {
        try {
            const response = await fetch(`/api/${resourceId}/status`);
            if (!response.ok) {
                throw new Error(`Status request failed: ${response.status}`);
            }

            const status = await response.json();
            if (status.complete) {
                callback(status);
                return;
            }

            if (++attempts >= maxPolls) {
                callback({ error: 'Timeout' });
                return;
            }

            timer = setTimeout(poll, interval * 2 ** Math.min(attempts, 5));
        } catch (error) {
            callback({ error: error.message });
        }
    };

    poll();
    return () => clearTimeout(timer);
}")),

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
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Security", "exec runs the built string through a shell", "Unlike spawn with an argument array, exec passes the whole string to /bin/sh. An argument of \"; rm -rf /\" or \"$(curl attacker)\" is interpreted as shell syntax and executed. This is textbook command injection.", "critical", 3),
            new StoredReviewIssue("2", "Security", "The command allow-list was deleted", "Any binary on the PATH can now be invoked, not just the five read-only commands the old code permitted.", "high", 3),
            new StoredReviewIssue("3", "Security", "Argument sanitisation was deleted", "The removed replace stripped shell metacharacters. Nothing filters the arguments now, which is what makes the exec call exploitable.", "high", 3),
            new StoredReviewIssue("4", "Maintainability", "No output, error or exit-code handling", "exec is called without a callback and its ChildProcess is returned raw, so failures are invisible to the caller.", "low", 1)
        ],
        @"const { spawn } = require('child_process');

const ALLOWED_COMMANDS = new Set(['ls', 'cat', 'grep', 'head', 'tail']);

function executeSystemCommand(command, args) {
    if (!ALLOWED_COMMANDS.has(command)) {
        throw new Error('Command not allowed');
    }

    const safeArgs = args.map((arg) => arg.replace(/[^a-zA-Z0-9._-]/g, ''));

    // spawn with an argument array never involves a shell.
    return spawn(command, safeArgs, { stdio: 'pipe', shell: false });
}"))
        };

        // Providers are constructed by DI; no static instance is required.

        public MediumJavaScriptCodeReviewProblems()
            : base(_problems, Language.JavaScript, "js_medium", DifficultyLevel.Medium)
        {
        }
}
