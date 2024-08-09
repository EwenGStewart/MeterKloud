export function initialize() {
    let meterKloudIndexedDb = indexedDB.open(DATABASE_NAME, CURRENT_VERSION);
    meterKloudIndexedDb.onupgradeneeded = function () {
        let db = meterKloudIndexedDb.result;
        db.createObjectStore("sites", { keyPath: "id" });
        db.createObjectStore("sitedays", { keyPath: "id" });

    }
}



export function set(collectionName, value) {
    let meterKloudIndexedDb = indexedDB.open(DATABASE_NAME, CURRENT_VERSION);

    meterKloudIndexedDb.onsuccess = function () {
        let transaction = meterKloudIndexedDb.result.transaction(collectionName, "readwrite");
        let collection = transaction.objectStore(collectionName)
        collection.put(value);
    }
}

export async function get(collectionName, id) {
    let request = new Promise((resolve) => {
        let meterKloudIndexedDb = indexedDB.open(DATABASE_NAME, CURRENT_VERSION);
        meterKloudIndexedDb.onsuccess = function () {
            let transaction = meterKloudIndexedDb.result.transaction(collectionName, "readonly");
            let collection = transaction.objectStore(collectionName);
            let result = collection.get(id);

            result.onsuccess = function (e) {
                resolve(result.result);
            }
        }
    });

    let result = await request;

    return result;
}



export async function getAll(collectionName) {
    let request = new Promise((resolve) => {
        let meterKloudIndexedDb = indexedDB.open(DATABASE_NAME, CURRENT_VERSION);
        meterKloudIndexedDb.onsuccess = function () {
            let transaction = meterKloudIndexedDb.result.transaction(collectionName, "readonly");
            let collection = transaction.objectStore(collectionName);
            let result = collection.getAll();

            result.onsuccess = function (e) {
                resolve(result.result);
            }
        }
    });

    let result = await request;

    return result;
}

export async function getRange(collectionName, fromValue, toValue) {
    let request = new Promise((resolve) => {
        let meterKloudIndexedDb = indexedDB.open(DATABASE_NAME, CURRENT_VERSION);
        meterKloudIndexedDb.onsuccess = function () {
            let transaction = meterKloudIndexedDb.result.transaction(collectionName, "readonly");
            let collection = transaction.objectStore(collectionName);
            let result = collection.getAll(IDBKeyRange.bound(fromValue, toValue));

            result.onsuccess = function (e) {
                resolve(result.result);
            }
        }
    });

    let result = await request;

    return result;
}


// get with skip and take 



export async function remove(collectionName, id) {
    let request = new Promise((resolve) => {
        let meterKloudIndexedDb = indexedDB.open(DATABASE_NAME, CURRENT_VERSION);
        meterKloudIndexedDb.onsuccess = function () {
            let transaction = meterKloudIndexedDb.result.transaction(collectionName, "readwrite");
            let collection = transaction.objectStore(collectionName);
            let result = collection.delete(id);

            result.onsuccess = function (e) {
                resolve(result.result);
            }
        }
    });

    let result = await request;

    return result;
}

let CURRENT_VERSION = 1;
let DATABASE_NAME = "meterkloud";