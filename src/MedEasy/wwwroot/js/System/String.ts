interface String
{
    /**
    * Converts the string to its "Title Case" representation
    */
    toTitleCase(): string;

    format(...args: Array<any>);
}

String.prototype.toTitleCase = () => this.replace(/\w\S*/g,
    (txt: string) => txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase());

String.prototype.format = (...args: Array<any>) => { };
