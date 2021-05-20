const MetaHash = require('./src/');

module.exports = {
    sendTx: (callback, privateKey, to, value, data) => {
		
        const Wallet = MetaHash.Wallet;
        const API = MetaHash.API;

        const api = new API();
        const wallet = Wallet.fromPrivateKey(privateKey);

        api.getNonce({
            address: wallet.address
        }).then((nonce) => {

            const tx = wallet.createTx({
                to,
                value,
                fee,
                nonce,
                data
            });

            api.sendTx(tx).then((response, fail) => {
                callback(null,
                    {
                        transactionId: result.params,
                        json: JSON.stringify(result, null, 4)
                    });
            }, (err) => {
                callback(null,
                    {
                        json: JSON.stringify(err, null, 4)
                    });
            });
        });
    }
}