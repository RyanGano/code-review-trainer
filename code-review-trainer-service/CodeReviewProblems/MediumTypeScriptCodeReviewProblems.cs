namespace code_review_trainer_service.CodeReviewProblems;

public sealed class MediumTypeScriptCodeReviewProblems : CodeReviewProblems
{
    private static readonly ProblemDefinition[] _problems = new ProblemDefinition[]
    {
        // Patch example: original vs patched (Medium TS)
    new ProblemDefinition("Refactor non-empty check for clarity",
        @" function isNonEmpty(s?: string): boolean {
-    return !!s && s.length > 0;
+    if (s && s.length > 0) return false;
+    return true;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "The result is inverted", "isNonEmpty('hello') now returns false and isNonEmpty('') returns true. The commit describes a clarity refactor but changes what the function means.", "critical", 3),
            new StoredReviewIssue("2", "Maintainability", "if/return replaces a boolean expression", "The original expression was already the clearest form. Wrapping a boolean in an if that returns literals is a well-known anti-pattern.", "low", 1)
        ],
        @"function isNonEmpty(s?: string): boolean {
    return !!s && s.length > 0;
}")),

        // Problem 1: Security vulnerability - insufficient input sanitization and XSS
    new ProblemDefinition("Add renderUserProfile function",
            @" function renderUserProfile(user: any) {
-    const sanitizedName = user.name.replace(/[<>]/g, '');
-    const sanitizedBio = user.bio.replace(/[<>]/g, '');
+    const sanitizedName = user.name;
+    const sanitizedBio = user.bio;
     return `<div class=""profile"">
-        <h1>${sanitizedName}</h1>
-        <p>${sanitizedBio}</p>
+        <h1>${user.name}</h1>
+        <p>${user.bio}</p>
     </div>`;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Security", "User fields are interpolated into HTML unescaped", "Both the escaping and the use of the escaped values were removed, so a bio containing an img tag with an onerror handler executes in every viewer's session. This is stored XSS.", "critical", 3),
            new StoredReviewIssue("2", "Maintainability", "The variables named sanitized no longer sanitize", "sanitizedName and sanitizedBio are now plain copies that nothing reads. A future reader will trust the name and assume the output is safe.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "user is typed as any", "The parameter type gives up all checking, so a missing name is only discovered at runtime.", "medium", 2)
        ],
        @"interface UserProfile {
    name: string;
    bio: string;
}

function renderUserProfile(user: UserProfile): HTMLDivElement {
    const wrapper = document.createElement('div');
    wrapper.className = 'profile';

    const heading = document.createElement('h1');
    heading.textContent = user.name;

    const bio = document.createElement('p');
    bio.textContent = user.bio;

    wrapper.append(heading, bio);
    return wrapper;
}")),

        // Problem 2: Type safety issue - using any type instead of proper typing
    new ProblemDefinition("Add calculateTotal function",
            @" interface Product {
     id: number;
     name: string;
     price: number;
 }

 function calculateTotal(products: any[]) {
-    let total = 0;
-    for (const product of products) {
-        if (product.price && typeof product.price === 'number') {
-            total += product.price;
-        }
-    }
+    let total = 0;
+    for (const product of products) {
+        total += product.price;
+    }
     return total;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "The runtime type guard was removed while the parameter is still any[]", "Product is declared directly above but never used, so nothing checks price at compile time either. A product whose price is undefined or a string turns total into NaN or a concatenated string and the caller gets no error.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "any[] discards the interface that exists for this", "Typing the parameter as Product[] makes the removed guard unnecessary rather than merely absent, and documents what the function accepts.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "No return type annotation", "The function returns number and should say so.", "low", 1)
        ],
        @"function calculateTotal(products: readonly Product[]): number {
    return products.reduce((total, product) => total + product.price, 0);
}")),

        // Problem 3: Complex state management - improper state updates in React-like component
    new ProblemDefinition("Add CounterComponent class",
            @" class CounterComponent {
     private state = { count: 0, loading: false };

-    async increment() {
-        this.setState({ loading: true });
-        await new Promise(resolve => setTimeout(resolve, 100));
-        this.setState({ count: this.state.count + 1, loading: false });
-    }
-
-    private setState(updates: Partial<typeof this.state>) {
-        this.state = { ...this.state, ...updates };
-    }
+    async increment() {
+        this.state.loading = true;
+        await new Promise(resolve => setTimeout(resolve, 100));
+        this.state.count++;
+        this.state.loading = false;
+    }

     getState() {
         return this.state;
     }
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "State is mutated in place instead of replaced", "The setState indirection was the only thing producing a new object on each change. Writing this.state.loading directly means the object identity never changes, so any consumer comparing previous and next state sees no update and never re-renders.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "loading is not reset if the await throws", "There is no try/finally, so a rejection between the two assignments leaves loading stuck true forever.", "medium", 2),
            new StoredReviewIssue("3", "Correctness", "Concurrent increments interleave", "Two overlapping calls both read this.state.count around the same await, so one of the increments is lost.", "medium", 2),
            new StoredReviewIssue("4", "Maintainability", "getState hands out the mutable internal object", "Callers can write to this.state through the returned reference, which defeats the point of having an update path at all.", "low", 1)
        ],
        @"class CounterComponent {
    private state = { count: 0, loading: false };

    async increment(): Promise<void> {
        this.setState({ loading: true });
        try {
            await new Promise((resolve) => setTimeout(resolve, 100));
            this.setState({ count: this.state.count + 1 });
        } finally {
            this.setState({ loading: false });
        }
    }

    private setState(updates: Partial<{ count: number; loading: boolean }>): void {
        this.state = { ...this.state, ...updates };
    }

    getState(): Readonly<{ count: number; loading: boolean }> {
        return this.state;
    }
}")),

        // Problem 4: Complex caching strategy - improper cache invalidation and memory leaks
    new ProblemDefinition("Add SmartCache class",
            @" interface CacheEntry<T> {
     data: T;
     timestamp: number;
     accessCount: number;
     size: number;
 }

 class SmartCache<T> {
-    private cache = new Map<string, CacheEntry<T>>();
-    private maxSize = 100;
-    private ttl = 300000; // 5 minutes
-
-    set(key: string, value: T) {
-        const entry: CacheEntry<T> = {
-            data: value,
-            timestamp: Date.now(),
-            accessCount: 0,
-            size: JSON.stringify(value).length
-        };
-        this.cache.set(key, entry);
-
-        if (this.cache.size > this.maxSize) {
-            this.evictOldest();
-        }
-    }
-
-    get(key: string): T | undefined {
-        const entry = this.cache.get(key);
-        if (entry && Date.now() - entry.timestamp < this.ttl) {
-            entry.accessCount++;
-            return entry.data;
-        }
-    }
-
-    private evictOldest() {
-        let oldestKey = '';
-        let oldestTime = Date.now();
-
-        for (const [key, entry] of this.cache) {
-            if (entry.timestamp < oldestTime) {
-                oldestTime = entry.timestamp;
-                oldestKey = key;
-            }
-        }
-
-        if (oldestKey) {
-            this.cache.delete(oldestKey);
-        }
-    }
+    private cache = new Map<string, CacheEntry<T>>();
+    private maxSize = 100;
+    private ttl = 300000; // 5 minutes
+
+    set(key: string, value: T) {
+        const entry: CacheEntry<T> = {
+            data: value,
+            timestamp: Date.now(),
+            accessCount: 0,
+            size: JSON.stringify(value).length
+        };
+        this.cache.set(key, entry);
+    }
+
+    get(key: string): T | undefined {
+        const entry = this.cache.get(key);
+        if (entry) {
+            entry.accessCount++;
+            return entry.data;
+        }
+    }
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "The TTL is declared but never enforced", "get now returns any entry it finds regardless of age, so a value cached once is served forever. The ttl field remains only as documentation of behaviour the class no longer has.", "high", 3),
            new StoredReviewIssue("2", "Performance", "Nothing evicts entries any more", "The size check and evictOldest were both removed, so the Map grows without bound. maxSize is now dead state and the cache is a memory leak with a lookup API.", "high", 3),
            new StoredReviewIssue("3", "Maintainability", "size and accessCount are computed but unused", "Every set pays for a full JSON.stringify to record a size nothing reads, and accessCount is incremented for no consumer.", "medium", 2),
            new StoredReviewIssue("4", "Maintainability", "get has no explicit undefined return", "The function falls off the end when there is no entry. Under noImplicitReturns that is an error, and either way an explicit return states the intent.", "low", 1)
        ],
        @"class SmartCache<T> {
    private readonly cache = new Map<string, CacheEntry<T>>();

    constructor(private readonly maxSize = 100, private readonly ttl = 300_000) {}

    set(key: string, value: T): void {
        this.cache.delete(key);
        this.cache.set(key, { data: value, timestamp: Date.now(), accessCount: 0, size: 0 });

        // Map preserves insertion order, so the first key is the oldest.
        while (this.cache.size > this.maxSize) {
            const oldest = this.cache.keys().next();
            if (oldest.done) {
                break;
            }
            this.cache.delete(oldest.value);
        }
    }

    get(key: string): T | undefined {
        const entry = this.cache.get(key);
        if (!entry) {
            return undefined;
        }

        if (Date.now() - entry.timestamp >= this.ttl) {
            this.cache.delete(key);
            return undefined;
        }

        entry.accessCount++;
        return entry.data;
    }
}")),

        // Problem 5: Performance issue - unnecessary object creation in loop
    new ProblemDefinition("Add processUsers function",
            @" interface User {
     id: number;
     name: string;
     email: string;
 }

 function processUsers(users: User[]) {
-    const results = [];
-    const format = new Intl.NumberFormat('en-US');
-    for (const user of users) {
-        results.push({
-            id: user.id,
-            displayName: user.name.toUpperCase(),
-            contact: user.email,
-            label: format.format(user.id)
-        });
-    }
+    const results = [];
+    for (const user of users) {
+        const format = new Intl.NumberFormat('en-US');
+        const mapper = (u: User) => ({
+            id: u.id,
+            displayName: u.name.toUpperCase(),
+            contact: u.email,
+            label: format.format(u.id)
+        });
+        results.push(mapper(user));
+    }
     return results;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Performance", "A new Intl.NumberFormat is constructed on every iteration", "Building a formatter is one of the more expensive operations in the Intl API because it resolves locale data. It was correctly hoisted out of the loop before this patch moved it inside, so a list of 10,000 users now builds 10,000 formatters.", "high", 3),
            new StoredReviewIssue("2", "Performance", "A closure is allocated per user for no reason", "mapper is redefined each iteration and called exactly once. Inlining the object literal, or hoisting the function, removes the allocation entirely.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "results is implicitly any[]", "const results = [] with no annotation infers any[], so nothing checks the shape being pushed and the return type is any[] as well.", "medium", 2)
        ],
        @"interface ProcessedUser {
    id: number;
    displayName: string;
    contact: string;
    label: string;
}

function processUsers(users: readonly User[]): ProcessedUser[] {
    const format = new Intl.NumberFormat('en-US');

    return users.map((user) => ({
        id: user.id,
        displayName: user.name.toUpperCase(),
        contact: user.email,
        label: format.format(user.id),
    }));
}")),

        // Problem 6: Async error handling - unhandled promise rejection
    new ProblemDefinition("Add fetchUserData function",
            @" async function fetchUserData(userId: number) {
-    try {
-        const response = await fetch(`/api/users/${userId}`);
-        const data = await response.json();
-        return data;
-    } catch (error) {
-        console.error('Failed to fetch user:', error);
-        throw error;
-    }
+    const response = await fetch(`/api/users/${userId}`);
+    const data = await response.json();
+    return data;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "HTTP error responses are treated as success", "fetch only rejects on network failure, so a 404 or 500 reaches response.json() and either throws a confusing parse error or returns an error body typed as user data. Nothing in this function checks response.ok.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "The diagnostic log was removed", "The catch existed to record which user id failed before rethrowing. Without it a failure surfaces up the stack with no context about the request that caused it.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "The return type is any", "response.json() produces any, which propagates to every caller of fetchUserData.", "medium", 2)
        ],
        @"interface UserData {
    id: number;
    email: string;
}

async function fetchUserData(userId: number): Promise<UserData> {
    const response = await fetch(`/api/users/${userId}`);
    if (!response.ok) {
        throw new Error(`Failed to fetch user ${userId}: ${response.status}`);
    }

    return (await response.json()) as UserData;
}")),

        // Problem 7: Complex algorithm inefficiency - multiple array operations in sequence
    new ProblemDefinition("Add processLargeDataset function",
            @" interface DataItem {
     id: number;
     category: string;
     value: number;
     active: boolean;
 }

 function processLargeDataset(items: DataItem[]) {
-    const result = new Set<number>();
-    for (const item of items) {
-        if (item.active) {
-            result.add(item.value);
-        }
-    }
-    return Array.from(result).sort((a, b) => a - b);
+    // Filter active items
+    const activeItems = items.filter(item => item.active);
+    // Map to values
+    const values = activeItems.map(item => item.value);
+    // Sort values
+    const sortedValues = values.sort((a, b) => a - b);
+    // Remove duplicates
+    const uniqueValues = sortedValues.filter((val, idx) => sortedValues.indexOf(val) === idx);
+    return uniqueValues;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Performance", "indexOf inside filter makes deduplication quadratic", "The last step scans the whole array for every element, so on a large dataset - the case the function is named for - this is O(n squared) where the Set it replaced was O(n).", "high", 3),
            new StoredReviewIssue("2", "Performance", "Four passes and three intermediate arrays", "filter, map, sort and filter each walk the data and allocate. The single loop plus Set did it in one pass with one allocation.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "Comments restate the method names", "// Filter active items above a filter call adds nothing a reader cannot already see.", "trivial", 1)
        ],
        @"function processLargeDataset(items: readonly DataItem[]): number[] {
    const values = new Set<number>();
    for (const item of items) {
        if (item.active) {
            values.add(item.value);
        }
    }

    return [...values].sort((a, b) => a - b);
}")),

        // Problem 8: Complex event handling - memory leaks with improper cleanup
    new ProblemDefinition("Add EventManager class",
            @" class EventManager {
-    private handlers = new Map<string, Set<Function>>();
-
-    addHandler(event: string, handler: Function) {
-        if (!this.handlers.has(event)) {
-            this.handlers.set(event, new Set());
-        }
-        this.handlers.get(event)!.add(handler);
-    }
-
-    removeHandler(event: string, handler: Function) {
-        this.handlers.get(event)?.delete(handler);
-    }
-
-    emit(event: string, data?: any) {
-        this.handlers.get(event)?.forEach(handler => handler(data));
-    }
-
-    clear() {
-        this.handlers.clear();
-    }
+    private handlers = new Map<string, Function[]>();
+
+    addHandler(event: string, handler: Function) {
+        if (!this.handlers.has(event)) {
+            this.handlers.set(event, []);
+        }
+        this.handlers.get(event)!.push(handler);
+    }
+
+    emit(event: string, data?: any) {
+        const eventHandlers = this.handlers.get(event);
+        if (eventHandlers) {
+            eventHandlers.forEach(handler => handler(data));
+        }
+    }
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Both removeHandler and clear were deleted", "There is now no way to unsubscribe. Every handler, and everything it closes over, is retained for the lifetime of the EventManager - the standard listener leak, and it is silent.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "An array allows the same handler to register twice", "The Set deduplicated subscriptions. With an array, a component that subscribes on every render fires its handler once per registration.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "Handlers are typed as Function and data as any", "Neither the arguments nor the return value are checked at any call site.", "low", 1)
        ],
        @"type Handler<T> = (data: T) => void;

class EventManager<Events extends Record<string, unknown>> {
    private handlers = new Map<keyof Events, Set<Handler<never>>>();

    addHandler<K extends keyof Events>(event: K, handler: Handler<Events[K]>): () => void {
        const set = this.handlers.get(event) ?? new Set();
        set.add(handler as Handler<never>);
        this.handlers.set(event, set);
        return () => this.removeHandler(event, handler);
    }

    removeHandler<K extends keyof Events>(event: K, handler: Handler<Events[K]>): void {
        this.handlers.get(event)?.delete(handler as Handler<never>);
    }

    emit<K extends keyof Events>(event: K, data: Events[K]): void {
        this.handlers.get(event)?.forEach((handler) => (handler as Handler<Events[K]>)(data));
    }

    clear(): void {
        this.handlers.clear();
    }
}")),

        // Problem 9: Memory leak - closures capturing large objects
    new ProblemDefinition("Add createHandlers function",
            @" function createHandlers(largeData: any[]) {
-    const handlers = [];
-    for (let i = 0; i < largeData.length; i++) {
-        const label = `Item ${i}: ${largeData[i].id}`;
-        handlers.push(() => {
-            console.log(label);
-        });
-    }
+    const handlers = [];
+    for (let i = 0; i < largeData.length; i++) {
+        handlers.push(() => {
+            console.log(`Item ${i}:`, largeData);
+        });
+    }
     return handlers;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Performance", "Every closure now captures the entire dataset", "The old body extracted a small string per item so the array could be collected once the handlers were built. Each new closure references largeData directly, so a single surviving handler keeps the whole dataset alive.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "The log now dumps the whole array per handler", "console.log(`Item ${i}:`, largeData) prints the full dataset on every invocation rather than the one item that was intended.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "largeData is any[] and handlers is implicitly any[]", "Neither the input nor the output is typed, so nothing here is checked.", "low", 1)
        ],
        @"function createHandlers(largeData: readonly { id: string }[]): (() => void)[] {
    // Capture only the small string each handler needs.
    return largeData.map((item, index) => {
        const label = `Item ${index}: ${item.id}`;
        return () => console.log(label);
    });
}")),

        // Problem 10: Complex async error handling - error swallowing in promise chains
    new ProblemDefinition("Add processUserData function",
            @" interface UserData {
     id: number;
     email: string;
     preferences: any;
 }

 async function processUserData(userIds: number[]) {
-    const results = [];
-    for (const userId of userIds) {
-        try {
-            const userData = await fetchUser(userId);
-            const processed = await processPreferences(userData.preferences);
-            const validated = await validateEmail(userData.email);
-            results.push({ ...userData, processed, validated });
-        } catch (error) {
-            console.error(`Failed to process user ${userId}:`, error);
-            results.push({ id: userId, error: 'Processing failed' });
-        }
-    }
-    return results;
+    const results = [];
+    const promises = userIds.map(async (userId) => {
+        const userData = await fetchUser(userId);
+        const processed = await processPreferences(userData.preferences);
+        const validated = await validateEmail(userData.email);
+        return { ...userData, processed, validated };
+    });
+
+    const settled = await Promise.all(promises.map(p => p.catch(() => null)));
+    for (const item of settled) {
+        if (item) {
+            results.push(item);
+        }
+    }
+    return results;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "catch(() => null) discards the error entirely", "The rejection reason is thrown away with no log and no rethrow, so a failure is indistinguishable from a user that simply was not returned. The removed version logged the id and the error.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "Failed users vanish from the result", "Nulls are filtered out, so the caller receives a shorter array with no way to tell which ids failed. The old code pushed an explicit error entry per user.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "Promise.allSettled expresses this directly", "Mapping catch onto every promise to fake allSettled is exactly what allSettled does, and it preserves the reason.", "low", 1)
        ],
        @"async function processUserData(userIds: readonly number[]) {
    const settled = await Promise.allSettled(
        userIds.map(async (userId) => {
            const userData = await fetchUser(userId);
            const [processed, validated] = await Promise.all([
                processPreferences(userData.preferences),
                validateEmail(userData.email),
            ]);
            return { ...userData, processed, validated };
        }),
    );

    return settled.map((outcome, index) => {
        if (outcome.status === 'fulfilled') {
            return outcome.value;
        }

        console.error(`Failed to process user ${userIds[index]}:`, outcome.reason);
        return { id: userIds[index], error: 'Processing failed' };
    });
}")),

        // Problem 11: Race condition - concurrent access to shared state
    new ProblemDefinition("Add Counter class",
            @" class Counter {
-    private count = 0;
-
-    increment() {
-        this.count++;
-    }
-
-    getCount() {
-        return this.count;
-    }
+    private count = 0;

+    async increment() {
+        const current = this.count;
+        await new Promise(resolve => setTimeout(resolve, 1));
+        this.count = current + 1;
+    }

+    getCount() {
+        return this.count;
+    }
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Read-modify-write across an await loses increments", "The value is read before the await and written after it. Ten overlapping calls to increment all read 0 and all write 1, so the counter ends at 1 instead of 10. The synchronous ++ it replaced could not interleave.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "increment became async for no stated reason", "Every existing call site now gets a floating promise unless it is updated to await, and nothing in the commit explains what the delay is for.", "medium", 2)
        ],
        @"class Counter {
    private count = 0;

    increment(): void {
        this.count++;
    }

    getCount(): number {
        return this.count;
    }
}")),

        // Problem 12: Complex concurrency issue - improper handling of shared resources
    new ProblemDefinition("Add DatabaseConnectionPool class",
            @" class DatabaseConnectionPool {
-    private availableConnections: any[] = [];
-    private waitingQueue: ((conn: any) => void)[] = [];
-    private maxConnections = 10;
-    private activeConnections = 0;
-
-    async getConnection() {
-        if (this.availableConnections.length > 0) {
-            return this.availableConnections.pop();
-        }
-
-        if (this.activeConnections < this.maxConnections) {
-            this.activeConnections++;
-            return await this.createConnection();
-        }
-
-        return new Promise((resolve) => {
-            this.waitingQueue.push(resolve);
-        });
-    }
-
-    releaseConnection(conn: any) {
-        if (this.waitingQueue.length > 0) {
-            const waitingResolver = this.waitingQueue.shift();
-            waitingResolver(conn);
-        } else {
-            this.availableConnections.push(conn);
-        }
-    }
+    private connections: any[] = [];
+    private maxConnections = 10;

+    async getConnection() {
+        if (this.connections.length < this.maxConnections) {
+            const conn = await this.createConnection();
+            this.connections.push(conn);
+            return conn;
+        }

+        return new Promise((resolve) => {
+            const checkConnections = () => {
+                if (this.connections.length > 0) {
+                    resolve(this.connections.pop());
+                } else {
+                    setTimeout(checkConnections, 100);
+                }
+            };
+            checkConnections();
+        });
+    }

+    releaseConnection(conn: any) {
+        this.connections.push(conn);
+    }
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "A connection is handed out and left in the pool at the same time", "getConnection pushes the new connection into this.connections and also returns it, so the same handle is simultaneously in use and considered available. Two callers end up sharing one connection and interleaving statements on it.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "The waiting path steals a connection that is already in use", "Once the pool is full, checkConnections pops whatever is in the array - which, per the previous issue, is a connection somebody else is using right now.", "high", 3),
            new StoredReviewIssue("3", "Performance", "Busy-wait polling replaced an explicit queue", "The removed waitingQueue woke a caller the moment a connection was released. The setTimeout loop adds up to 100ms of latency per attempt, gives no fairness between waiters, and never times out.", "medium", 2),
            new StoredReviewIssue("4", "Maintainability", "Connections are typed as any", "Nothing checks that what comes back into releaseConnection is a connection at all.", "low", 1)
        ],
        @"class DatabaseConnectionPool {
    private readonly available: Connection[] = [];
    private readonly waiting: ((conn: Connection) => void)[] = [];
    private created = 0;

    constructor(private readonly maxConnections = 10) {}

    async getConnection(): Promise<Connection> {
        const pooled = this.available.pop();
        if (pooled) {
            return pooled;
        }

        if (this.created < this.maxConnections) {
            this.created++;
            return this.createConnection();
        }

        return new Promise<Connection>((resolve) => this.waiting.push(resolve));
    }

    releaseConnection(conn: Connection): void {
        const next = this.waiting.shift();
        if (next) {
            next(conn);
        } else {
            this.available.push(conn);
        }
    }
}")),

        // Problem 13: Null safety issue - potential null reference exception
    new ProblemDefinition("Add getUserName function",
            @" interface User {
     id: number;
     profile?: {
         name?: string;
     };
 }

 function getUserName(user: User | null) {
-    if (user && user.profile && user.profile.name) {
-        return user.profile.name;
-    }
-    return 'Unknown';
+    return user!.profile!.name!;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Three non-null assertions silence checks the types were doing correctly", "The ! operator removes the compiler's objection without changing the runtime. getUserName(null) throws, and so does a user with no profile. The removed branch returned 'Unknown' for exactly those cases.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "The inferred return type is now a lie", "name is optional, so the expression can produce undefined while the signature says string. Every caller is told it has a string.", "medium", 2)
        ],
        @"function getUserName(user: User | null): string {
    return user?.profile?.name ?? 'Unknown';
}")),

        // Problem 14: Complex error handling - generic catch blocks hiding specific errors
    new ProblemDefinition("Add ApiClient class",
            @" class ApiClient {
     private baseUrl: string;

     constructor(baseUrl: string) {
         this.baseUrl = baseUrl;
     }

     async get(endpoint: string) {
         try {
             const response = await fetch(`${this.baseUrl}${endpoint}`);
-            if (!response.ok) {
-                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
-            }
             return await response.json();
         } catch (error) {
-            if (error instanceof TypeError) {
-                throw new Error('Network error: Unable to connect to server');
-            }
-            throw error;
+            throw new Error('API request failed');
         }
     }

     async post(endpoint: string, data: any) {
         try {
             const response = await fetch(`${this.baseUrl}${endpoint}`, {
                 method: 'POST',
                 headers: { 'Content-Type': 'application/json' },
                 body: JSON.stringify(data)
             });
-            if (!response.ok) {
-                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
-            }
             return await response.json();
         } catch (error) {
-            if (error instanceof TypeError) {
-                throw new Error('Network error: Unable to connect to server');
-            }
-            throw error;
+            throw new Error('API request failed');
         }
     }
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Every failure is flattened into one message with no cause", "A DNS failure, a 401 and a malformed JSON body all now throw 'API request failed'. The original error is not attached, so nothing downstream can retry intelligently and nothing in the logs says what happened.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "The response.ok check was removed from both methods", "fetch resolves for 4xx and 5xx, so an error page reaches response.json() and fails there instead of being reported as the HTTP error it is.", "high", 3),
            new StoredReviewIssue("3", "Maintainability", "Return types are any and post takes any", "Neither method says what it returns, so any is threaded through every call site.", "medium", 2)
        ],
        @"class ApiClient {
    constructor(private readonly baseUrl: string) {}

    async get<T>(endpoint: string): Promise<T> {
        return this.request<T>(endpoint);
    }

    async post<T>(endpoint: string, data: unknown): Promise<T> {
        return this.request<T>(endpoint, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data),
        });
    }

    private async request<T>(endpoint: string, init?: RequestInit): Promise<T> {
        let response: Response;
        try {
            response = await fetch(`${this.baseUrl}${endpoint}`, init);
        } catch (error) {
            throw new Error('Network error: Unable to connect to server', { cause: error });
        }

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        return (await response.json()) as T;
    }
}")),

        // Problem 15: Complex performance issue - inefficient string operations in tight loops
    new ProblemDefinition("Add generateReport function",
            @" interface ReportData {
     id: string;
     name: string;
     values: number[];
     metadata: Record<string, any>;
 }

 function generateReport(data: ReportData[]) {
-    const lines = ['ID,Name,Sum,Count,Average'];
-    for (const item of data) {
-        const sum = item.values.reduce((a, b) => a + b, 0);
-        const count = item.values.length;
-        const avg = count > 0 ? sum / count : 0;
-        lines.push(`${item.id},${item.name},${sum},${count},${avg}`);
-    }
-    return lines.join('\n');
+    let report = 'ID,Name,Sum,Count,Average\n';
+    for (const item of data) {
+        const sum = item.values.reduce((a, b) => a + b, 0);
+        const count = item.values.length;
+        const avg = count > 0 ? sum / count : 0;
+        report += `${item.id},${item.name},${sum},${count},${avg}\n`;
+    }
+    return report;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Performance", "The report is rebuilt by repeated concatenation", "report += ... allocates a new string containing everything written so far on each row. For a large report that is quadratic copying, which is exactly what the array plus join it replaced avoided.", "medium", 2),
            new StoredReviewIssue("2", "Correctness", "CSV fields are never escaped", "A name containing a comma or a quote breaks the column alignment of the whole file. This was true before the patch too, but it is the defect that matters most in the output.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "No return type annotation", "generateReport returns string and should say so.", "trivial", 1)
        ],
        @"function generateReport(data: readonly ReportData[]): string {
    const escape = (field: string) =>
        /["",\n]/.test(field) ? `""${field.replace(/""/g, '""""')}""` : field;

    const lines = ['ID,Name,Sum,Count,Average'];
    for (const item of data) {
        const sum = item.values.reduce((a, b) => a + b, 0);
        const count = item.values.length;
        const avg = count > 0 ? sum / count : 0;
        lines.push(`${escape(item.id)},${escape(item.name)},${sum},${count},${avg}`);
    }

    return lines.join('\n');
}")),

        // Problem 16: Array bounds issue - no bounds checking before access
    new ProblemDefinition("Add getElementAt function",
            @" function getElementAt<T>(array: T[], index: number): T {
-    if (index >= 0 && index < array.length) {
-        return array[index];
-    }
-    throw new Error('Index out of bounds');
+    return array[index];
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Out-of-range access now returns undefined typed as T", "TypeScript does not check index access unless noUncheckedIndexedAccess is on, so getElementAt(items, 99) hands back undefined while the signature promises T. The failure surfaces later, wherever that value is finally used.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "Negative and fractional indexes are accepted", "index is any number, so -1 and 1.5 silently produce undefined rather than being rejected the way the removed guard rejected them.", "medium", 2)
        ],
        @"function getElementAt<T>(array: readonly T[], index: number): T {
    if (!Number.isInteger(index) || index < 0 || index >= array.length) {
        throw new RangeError(`Index ${index} is out of bounds`);
    }

    return array[index];
}")),

        // Problem 17: Exception handling - catching generic Exception instead of specific types
    new ProblemDefinition("Add parseData function",
            @" function parseData(input: string) {
-    try {
-        return JSON.parse(input);
-    } catch (error) {
-        if (error instanceof SyntaxError) {
-            console.error('Invalid JSON:', error.message);
-            return null;
-        }
-        throw error;
-    }
+    try {
+        return JSON.parse(input);
+    } catch (error) {
+        console.error('Error parsing data:', error);
+        return null;
+    }
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Every error is now converted into null", "The removed code only handled SyntaxError and rethrew anything else. A RangeError from an oversized string, or an error thrown by a reviver, is now swallowed and reported as though the input were simply invalid JSON.", "medium", 2),
            new StoredReviewIssue("2", "Maintainability", "The return type is any", "JSON.parse returns any, so an untyped value spreads to every caller. unknown forces a check at the boundary.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "null gives the caller no reason for the failure", "Callers cannot distinguish valid JSON containing null from a parse failure.", "low", 1)
        ],
        @"function parseData(input: string): unknown {
    try {
        return JSON.parse(input);
    } catch (error) {
        if (error instanceof SyntaxError) {
            console.error('Invalid JSON:', error.message);
            return null;
        }

        throw error;
    }
}")),

        // Problem 18: Mutable default parameters causing shared state issues
    new ProblemDefinition("Add createConfig function",
            @"-function createConfig(options: any = {}) {
-    const defaults = { timeout: 5000, retries: 3 };
-    return { ...defaults, ...options };
+const DEFAULT_OPTIONS = { timeout: 5000, retries: 3 };
+
+function createConfig(options: any = DEFAULT_OPTIONS) {
+    return Object.assign(DEFAULT_OPTIONS, options);
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Object.assign writes into the shared defaults object", "The first argument is the mutation target, so createConfig({ timeout: 1 }) permanently changes DEFAULT_OPTIONS. Every later call - including ones that pass no options at all - inherits that timeout. The spread it replaced built a fresh object each time.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "Every caller receives the same object", "Because the target is returned, two configs are the same reference. Mutating one changes the other and changes the defaults.", "high", 3),
            new StoredReviewIssue("3", "Maintainability", "options is typed any", "Nothing checks the shape or catches a misspelled key such as timeOut.", "medium", 2)
        ],
        @"interface Config {
    timeout: number;
    retries: number;
}

const DEFAULT_OPTIONS: Readonly<Config> = { timeout: 5000, retries: 3 };

function createConfig(options: Partial<Config> = {}): Config {
    return { ...DEFAULT_OPTIONS, ...options };
}")),

        // Problem 19: Callback hell - deeply nested async operations
    new ProblemDefinition("Add processData function",
            @" function processData(input: string, callback: (result: any) => void) {
-    validateInput(input, (isValid) => {
-        if (isValid) {
-            parseData(input, (parsed) => {
-                transformData(parsed, callback);
-            });
-        } else {
-            callback(null);
-        }
-    });
+    validateInput(input, (isValid) => {
+        if (isValid) {
+            parseData(input, (parsed) => {
+                transformData(parsed, (transformed) => {
+                    saveData(transformed, (saved) => {
+                        auditSave(saved, (audited) => {
+                            notifyListeners(audited, (notified) => {
+                                callback(notified);
+                            });
+                        });
+                    });
+                });
+            });
+        } else {
+            callback(null);
+        }
+    });
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "No step can report an error", "None of the callbacks takes an error argument, so a failure in saveData, auditSave or notifyListeners either throws where nothing can catch it or silently never calls back and the caller waits forever.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "Six levels of nested callbacks", "The patch adds three more steps to an already nested chain. Each level indents further, the failure paths cannot be shared, and adding a seventh step means editing the innermost line of a pyramid.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "Results are typed any throughout", "The callback signature is (result: any) => void and every intermediate value is untyped.", "medium", 2)
        ],
        @"async function processData(input: string): Promise<unknown> {
    if (!(await validateInput(input))) {
        return null;
    }

    const parsed = await parseData(input);
    const transformed = await transformData(parsed);
    const saved = await saveData(transformed);
    const audited = await auditSave(saved);
    return notifyListeners(audited);
}")),

        // Problem 20: SQL injection vulnerability - direct string concatenation in queries
    new ProblemDefinition("Add findUser function",
            @" function findUser(username: string) {
-    const query = 'SELECT * FROM users WHERE username = ?';
-    return db.query(query, [username]);
+    const query = `SELECT * FROM users WHERE username = '${username}'`;
+    return db.query(query);
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Security", "Parameterised query replaced with string interpolation", "A username of ' OR '1'='1 returns every row, and one ending in a statement separator can run a second command. The placeholder form this patch removes was already immune to both.", "critical", 3),
            new StoredReviewIssue("2", "Maintainability", "SELECT * couples the caller to the table shape", "Listing the needed columns keeps the query stable as the schema grows and avoids handing password hashes to a caller that only wanted a display name.", "low", 1)
        ],
        @"function findUser(username: string) {
    return db.query('SELECT id, username, email FROM users WHERE username = ?', [username]);
}")),

        // Problem 21: Infinite loop - missing termination condition
    new ProblemDefinition("Add findIndex function",
            @" function findIndex<T>(array: T[], predicate: (item: T) => boolean): number {
     let index = 0;
-    while (index < array.length) {
+    while (true) {
         if (predicate(array[index])) {
             return index;
         }
         index++;
     }
-    return -1;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "The loop never terminates when nothing matches", "The bound was the only exit. With while (true) the index runs past the end of the array and keeps calling the predicate with undefined forever, hanging whatever called it.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "The not-found return was deleted", "return -1 was the documented contract for no match. Callers checking for -1 now wait instead, and under noImplicitReturns TypeScript flags the missing path.", "high", 3),
            new StoredReviewIssue("3", "Maintainability", "Array.prototype.findIndex already does this", "The standard method is correct, returns -1 and needs no index bookkeeping.", "low", 1)
        ],
        @"function findIndex<T>(array: readonly T[], predicate: (item: T) => boolean): number {
    return array.findIndex(predicate);
}")),

        // Problem 22: Floating point precision issues in financial calculations
    new ProblemDefinition("Add calculateTax function",
            @" function calculateTax(amount: number, rate: number): number {
-    const tax = amount * rate;
-    return Math.round(tax * 100) / 100;
+    return amount * rate;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Rounding to cents was removed from a money calculation", "calculateTax(19.99, 0.0825) now returns 1.6491749999999999. That value flows into totals and invoices, where sub-cent noise accumulates and stops the books balancing.", "medium", 2),
            new StoredReviewIssue("2", "Correctness", "Binary floating point is the wrong representation for currency", "Even with the rounding restored, values such as 0.1 cannot be represented exactly. Money belongs in integer cents or a decimal type, and this patch was the moment to say so.", "medium", 2)
        ],
        @"// Work in integer cents so no intermediate value can be un-representable.
function calculateTax(amountInCents: number, rate: number): number {
    return Math.round(amountInCents * rate);
}")),

        // Problem 23: Complex async/await issue - blocking operations in async functions
    new ProblemDefinition("Add FileProcessor class",
            @" class FileProcessor {
     private cache = new Map<string, string>();

     async processFile(filePath: string) {
         if (this.cache.has(filePath)) {
             return this.cache.get(filePath);
         }

-        const content = await fs.promises.readFile(filePath, 'utf8');
+        const content = fs.readFileSync(filePath, 'utf8');
         const processed = await this.processContent(content);
         this.cache.set(filePath, processed);
         return processed;
     }

     private async processContent(content: string) {
         // Simulate async processing
         return new Promise(resolve => {
             setTimeout(() => resolve(content.toUpperCase()), 100);
         });
     }
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Performance", "Synchronous file read inside an async method", "readFileSync blocks the event loop for the whole read, so every other request in the process stalls behind it. The async signature tells callers the opposite. The promises API it replaced did not block.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "processContent resolves to unknown but is stored in a Map<string, string>", "new Promise with no type argument produces Promise<unknown>, so this.cache.set(filePath, processed) does not satisfy the declared value type.", "medium", 2),
            new StoredReviewIssue("3", "Correctness", "Concurrent calls for the same path do the work twice", "The cache is only written after processing completes, so two callers arriving together both read and both process. Caching the promise avoids it.", "low", 1)
        ],
        @"class FileProcessor {
    private cache = new Map<string, Promise<string>>();

    async processFile(filePath: string): Promise<string> {
        const cached = this.cache.get(filePath);
        if (cached) {
            return cached;
        }

        const work = (async () => {
            const content = await fs.promises.readFile(filePath, 'utf8');
            return this.processContent(content);
        })();

        this.cache.set(filePath, work);
        return work;
    }

    private async processContent(content: string): Promise<string> {
        return new Promise<string>((resolve) => {
            setTimeout(() => resolve(content.toUpperCase()), 100);
        });
    }
}")),

        // Problem 24: Complex conditional logic - nested conditionals with side effects
    new ProblemDefinition("Add validateAndProcess function",
            @" interface ValidationResult {
     isValid: boolean;
     errors: string[];
     processedData?: any;
 }

 function validateAndProcess(data: any): ValidationResult {
-    const errors: string[] = [];
-
-    if (!data) {
-        errors.push('Data is required');
-    } else {
-        if (typeof data.id !== 'number') {
-            errors.push('ID must be a number');
-        }
-        if (!data.name || typeof data.name !== 'string') {
-            errors.push('Name must be a non-empty string');
-        }
-    }
-
-    if (errors.length === 0) {
-        return {
-            isValid: true,
-            errors: [],
-            processedData: { ...data, validatedAt: Date.now() }
-        };
-    }
-
-    return { isValid: false, errors };
+    const result: ValidationResult = { isValid: true, errors: [] };
+
+    if (data) {
+        if (typeof data.id === 'number') {
+            if (data.name && typeof data.name === 'string') {
+                if (data.name.length > 0) {
+                    result.processedData = { ...data, validatedAt: Date.now() };
+                } else {
+                    result.isValid = false;
+                    result.errors.push('Name cannot be empty');
+                }
+            } else {
+                result.isValid = false;
+                result.errors.push('Name must be a string');
+            }
+        } else {
+            result.isValid = false;
+            result.errors.push('ID must be a number');
+        }
+    } else {
+        result.isValid = false;
+        result.errors.push('Data is required');
+    }
+
+    return result;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Only the first problem is ever reported", "The nested else branches stop at the first failure, so a payload with a bad id and a missing name reports only the id. The flat version collected every error, which is what a form needs in order to show the user what to fix.", "medium", 2),
            new StoredReviewIssue("2", "Maintainability", "Four levels of nesting with mutation at each level", "isValid and errors are written from six different branches, so working out the final state means tracing every path. The guard-clause version had one place that decided the result.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "data and processedData are typed any", "Nothing is narrowed by the checks, so the validation buys the caller no type safety.", "medium", 2)
        ],
        @"function validateAndProcess(data: unknown): ValidationResult {
    const errors: string[] = [];
    const record = data as { id?: unknown; name?: unknown } | null;

    if (!record) {
        errors.push('Data is required');
    } else {
        if (typeof record.id !== 'number') {
            errors.push('ID must be a number');
        }
        if (typeof record.name !== 'string' || record.name.length === 0) {
            errors.push('Name must be a non-empty string');
        }
    }

    if (errors.length > 0) {
        return { isValid: false, errors };
    }

    return {
        isValid: true,
        errors: [],
        processedData: { ...(record as object), validatedAt: Date.now() },
    };
}")),

        // Problem 25: Complex array manipulation - incorrect slice usage and boundary issues
    new ProblemDefinition("Add DataProcessor class",
            @" class DataProcessor {
-    private data: number[] = [];
-
-    addNumbers(numbers: number[]) {
-        this.data.push(...numbers);
-    }
-
-    getMiddleSection(startPercent: number, endPercent: number) {
-        const start = Math.floor(this.data.length * startPercent / 100);
-        const end = Math.floor(this.data.length * endPercent / 100);
-        return this.data.slice(start, end);
-    }
-
-    removeOutliers(threshold: number) {
-        this.data = this.data.filter(num => Math.abs(num) <= threshold);
-    }
+    private data: number[] = [];

+    addNumbers(numbers: number[]) {
+        this.data.push(...numbers);
+    }

+    getMiddleSection(startPercent: number, endPercent: number) {
+        if (startPercent >= endPercent || startPercent < 0 || endPercent > 100) {
+            throw new Error('Invalid percentage range');
+        }
+        const start = Math.floor(this.data.length * startPercent / 100);
+        const end = Math.floor(this.data.length * endPercent / 100);
+        return this.data.slice(start, end + 1);
+    }

+    removeOutliers(threshold: number) {
+        this.data = this.data.filter(num => Math.abs(num) <= threshold);
+    }
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "slice(start, end + 1) returns one element too many", "slice already treats its second argument as exclusive, so adding one includes the element just past the requested range. Asking for 0-50% of a 100-item array returns 51 items, and consecutive sections overlap instead of tiling.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "push with spread throws on very large inputs", "addNumbers spreads the whole argument into the call, so a few hundred thousand numbers exceed the engine's argument limit and throw RangeError. A loop or concat has no such ceiling.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "No return type annotations", "Neither getMiddleSection nor the mutators declare what they produce.", "low", 1)
        ],
        @"class DataProcessor {
    private data: number[] = [];

    addNumbers(numbers: readonly number[]): void {
        for (const number of numbers) {
            this.data.push(number);
        }
    }

    getMiddleSection(startPercent: number, endPercent: number): number[] {
        if (startPercent >= endPercent || startPercent < 0 || endPercent > 100) {
            throw new RangeError('Invalid percentage range');
        }

        const start = Math.floor((this.data.length * startPercent) / 100);
        const end = Math.floor((this.data.length * endPercent) / 100);
        return this.data.slice(start, end);
    }

    removeOutliers(threshold: number): void {
        this.data = this.data.filter((num) => Math.abs(num) <= threshold);
    }
}")),

        // Good examples
    new ProblemDefinition("Add safeAdd function with input validation",
@"+function safeAdd(a: number, b: number): number {
+    if (typeof a !== 'number' || typeof b !== 'number') {
+        throw new Error('Both arguments must be numbers');
+    }
+    return a + b;
+}",
        new StoredReview(ReviewStatus.Approve, [])),
    new ProblemDefinition("Add fetchJson async function with error handling",
@"+async function fetchJson(url: string): Promise<any> {
+    try {
+        const response = await fetch(url);
+        if (!response.ok) {
+            throw new Error(`HTTP error! status: ${response.status}`);
+        }
+        return await response.json();
+    } catch (error) {
+        console.error('Failed to fetch JSON:', error);
+        throw error;
+    }
+}",
        new StoredReview(ReviewStatus.Approve, [])),
    new ProblemDefinition("Add pick utility with bounds checking",
@"+function pick<T>(arr: T[], index: number): T | undefined {
+    if (!Array.isArray(arr)) {
+        throw new Error('First argument must be an array');
+    }
+    if (index < 0 || index >= arr.length) {
+        return undefined;
+    }
+    return arr[index];
+}",
        new StoredReview(ReviewStatus.Approve, [])),
    new ProblemDefinition("Add formatName function with proper validation",
@"+function formatName(first: string, last: string): string {
+    if (!first?.trim() || !last?.trim()) {
+        throw new Error('Both first and last names are required');
+    }
+    return `${first.trim()} ${last.trim()}`;
+}",
        new StoredReview(ReviewStatus.Approve, [])),
    new ProblemDefinition("Add noop function with JSDoc documentation",
@"+/**
+ * A no-operation function that does nothing.
+ * Useful as a default callback or placeholder.
+ */
+function noop(): void {
+    // This function intentionally does nothing
+}",
        new StoredReview(ReviewStatus.Approve, [])),
    };

    // Providers are constructed by DI; no static instance is required.

    public MediumTypeScriptCodeReviewProblems()
        : base(_problems, Language.TypeScript, "ts_medium", DifficultyLevel.Medium)
    {
    }
}
