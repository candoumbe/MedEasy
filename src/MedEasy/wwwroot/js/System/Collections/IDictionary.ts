import { KeyValuePair } from "./KeyValuePair";
/***/
export interface IDictionary<TKey, TValue>
{

    /**
        * Entries currently in the 
        */
    entries(): Array<KeyValuePair<TKey, TValue>>;

    /**
        * Gets all keys of the dictionary
        */
    keys(): Array<TKey>;

    /***/
    values(): Array<TValue>;

    /**
        * Gets the number of entries currently in the dictionary
        */
    count(): number;

    /**
        * Checks if specified key is already presents in the IDictionary implementation
        * @param {TKey} key the key to look for
        */
    containsKey(key: TKey): boolean;


    /**
        * Checks if specified key is already presents in the IDictionary implementation
        * @param {KeyValuePair<TKey, TValue>} kv checks if the specified 
        */
    contains(kv: KeyValuePair<TKey, TValue>): boolean;


    value(key: TKey): TValue;


    /**
        * Add an entry in the dictionary.
        * This mayt throws a KeyAlreadyExistsException depending on the implementation
        * @param {TKey} key key
        * @param {TValue} value value
        */
    add(key: TKey, value: TValue);


    /**
    * Removes an the entry with the specified key from the dictionary.
    * This mayt throws a KeyAlreadyExistsException depending on the implementation
    * @param {TKey} key key
    */
    remove(key: TKey);


    /**
    * Removes all entries from the dictionary.
    */
    clear();
}
