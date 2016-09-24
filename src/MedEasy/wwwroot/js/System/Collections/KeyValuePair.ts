/**
 * An instance of this class represents 
 */
export class KeyValuePair<TKey, TValue>
{
    private _key: TKey;

    private _value: TValue;

    /**
        * Creates an instance of a KeyValuePair
        * @param key the key
        * @param value the value
        */
    public constructor(key: TKey, value: TValue)
    {
        this._key = key;
        this._value = value;
    }

    /**
     * Gets the key
     */
    public get key()
    {
        return this._key;
    }

    /**
     * Gets the value
     */
    public get value()
    {
        return this._value
    };
}
