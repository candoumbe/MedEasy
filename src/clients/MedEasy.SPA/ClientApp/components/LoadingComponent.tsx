import * as React from "react";

interface LoadingComponentProps {
    /** Text that the component will show while loading */
    text?: string | React.ReactHTMLElement<any>;
}

export class LoadingComponent extends React.PureComponent<LoadingComponentProps, {}>{

    /**
     * Builds a new {LoadingComponent}
     * @param {LoadingComponentProps} props properties
     */
    public constructor(props: LoadingComponentProps) {
        super(props);
    }


    public render(): JSX.Element {
        return (
            <div className="center-block" >
                <div className="alert alert-info" role="alert">
                    {this.props.text || "Loading ..."}
                </div>
            </div>
        )
    }
}