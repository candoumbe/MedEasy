import { Exception, ArgumentNullException } from "../Exceptions/Index";
import { KeyValuePair } from "./KeyValuePair";
import { IDictionary } from "./IDictionary";



export class KeyAlreadyPresentException extends Exception
{
    public constructor(keyName: string)
    {
        super("keyName", "The key already exists");
    }
}

/**
 * An associative key, value array that doesn't allow duplicate keys
 * @param {TKey} TKey type of the key
 * @param {TValue} TValue type of the value
 */
export abstract class DictionaryBase<TKey, TValue> implements IDictionary<TKey, TValue>
{

    private _keys: Array<TKey>;

    private _values: Array<TValue>;

    private _entries: Array<KeyValuePair<TKey, TValue>>

    private _changedSinceLastComputation: boolean;

    private _count: number;

    /**
     * Creates a new Dictionary instance
     */
    public constructor()
    {
        this._count = 0;
        this._changedSinceLastComputation = false;

    }

    /**
     * Counts the number of entries of the dictionary
     */
    public count(): number
    {
        return this._entries.length;
    }

    /**
     * Computes keys and values properties so they can be accessed separately
     */
    private compute()
    {
        this._values = [];
        this._keys = [];
        for (var i = 0; i < this._entries.length; i++)
        {
            this._keys.push(this._entries[i].key);
            this._values.push(this._entries[i].value);
        }

        this._changedSinceLastComputation = false;

    }

    /**
     * Gets all elements 
     */
    public entries(): Array<KeyValuePair<TKey, TValue>>
    {
        return this._entries;
    }

    /**
     * Gets all keys
     */
    public keys(): Array<TKey>
    {
        if (this._changedSinceLastComputation)
        {
            this.compute();
        }
        return this._keys;
    }

    /**
     * Gets all values
     */
    public values(): Array<TValue>
    {
        return this._values;
    }

    public containsKey(key: TKey): boolean
    {
        return this.keys().filter((value) => value === key).length > 0;
    }

    public contains(kv: KeyValuePair<TKey, TValue>): boolean
    {
        return this.entries().filter((entry) => entry.key === kv.key && entry.value === kv.value).length > 0;
    }

    public value(key: TKey): TValue
    {
        var result: TValue;

        var found = false;
        var i = 0;
        while (!found && i < this._entries.length)
        {
            var currentEntry = this._entries[i];
            found = currentEntry.key === key;
            if (found)
            {
                result = currentEntry.value;
            }

            i++;
        }

        return result;
    }

    public add(key: TKey, value: TValue)
    {
        if (!key)
        {
            throw new ArgumentNullException("key", "key cannot be null");
        }

        if (this.containsKey(key))
        {
            throw new KeyAlreadyPresentException(String(key));
        }

        this._changedSinceLastComputation = true;
        this._entries.push(new KeyValuePair(key, value));
    }


    public remove(key: TKey)
    {
        if (key === null)
        {
            throw new ArgumentNullException("key", "key cannot be null");
        }


        var found = false;
        var i = 0;
        while (!found && i < this._entries.length)
        {
            var currentEntry = this._entries[i];
            found = currentEntry.key === key;
            if (found)
            {
                delete this._entries[i];
                this._changedSinceLastComputation = true;

            }
            i++;
        }
    }

    public clear(): void
    {
        this._entries = [];
        this._values = [];
        this._keys = [];
    }

}
