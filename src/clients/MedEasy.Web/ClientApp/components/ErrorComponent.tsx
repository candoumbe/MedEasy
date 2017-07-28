import * as React from "react";

interface ErrorComponentProps {
    /** Text that the component will display */
    text?: string | React.ReactHTMLElement<any>;
}

export class ErrorComponent extends React.PureComponent<ErrorComponentProps, void>{

    /**
     * Builds a new ErrorComponent
     * @param {ErrorComponentProps} props properties
     */
    public constructor(props: ErrorComponentProps) {
        super(props);
    }


    public render(): JSX.Element {
        return (
            <div className="center-block" >
                <div className="alert alert-info" role="alert">
                    {this.props.text || "An error occured ..."}
                </div>
            </div>
        )
    }
}