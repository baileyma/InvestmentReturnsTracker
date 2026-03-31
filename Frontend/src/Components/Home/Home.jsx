import { useState, useEffect } from 'react';
import axiosInstance from '../../utils/axiosInstance';
import './Home.scss';
import { Link } from 'react-router-dom';

const Home = () => {
  const [accounts, setAccounts] = useState({});
  const [aggregateData, setAggregateData] = useState({});
  const [individualAccountData, setIndividualAccountData] = useState({});
  const [showDelete, setShowDelete] = useState({});

  const apiUrl = import.meta.env.VITE_API_URL;
  const nfObject = new Intl.NumberFormat('en-US');

  const getAllReturns = async () => {
    try {
      const response = await axiosInstance.get(`${apiUrl}/Returns/accountreturns`);
      return response.data;
    } catch (error) {
      console.error(`Error getting all returns: ${error}`);
    }
  };

  useEffect(() => {
    const fetchAllReturns = async () => {
      try {
        const responseReturns = await getAllReturns();
        setAggregateData(responseReturns.aggregateData);
        setIndividualAccountData(responseReturns.individualAccountData);
      } catch (error) {
        console.error('Error fetching returns:', error);
      }
    };
    fetchAllReturns();
  }, [accounts]);

  const getAccounts = async () => {
    try {
      const response = await axiosInstance.get(`${apiUrl}/Accounts`);

      return response.data;
    } catch (error) {
      console.error(`Error retrieving players: ${error}`);
    }
  };

  useEffect(() => {
    const fetchAccounts = async () => {
      try {
        const response = await getAccounts();
        setAccounts(response); // Wait for accounts to be set
      } catch (error) {
        console.error('Error fetching accounts:', error);
      }
    };
    fetchAccounts();
  }, []); // Runs only once on mount

  // !!!!
  // could i make this dynamic by calling from backend or setting to current datetime property that records when there is a new year?
  const yearsArray = [2021, 2022, 2023, 2024, 2025, 2026];

  const addAccount = async (e) => {
    const accountObj = {
      accName: e.target.accName.value,
      startValue: e.target.startVal.value,
    };
    try {
      const ID = await axios.post('http://localhost:8080/accounts', accountObj);
      const response = await getAccounts();
      setAccounts(response);
      return ID;
    } catch (error) {
      console.error(`Error adding new account: ${error}`);
    }
  };

  const deleteAccount = async (accID) => {
    try {
      const ID = await axios.delete(`http://localhost:8080/accounts/${accID}`);
      const response = await getAccounts();
      setAccounts(response);
      return ID;
    } catch (error) {
      console.error(`Error deleting account: ${error}`);
    }
  };

  const toggleDelete = (accountID) => {
    setShowDelete((previousState) => ({
      ...previousState,
      [accountID]: !previousState[accountID],
    }));
  };

  const generateExcel = async () => {
    const ID = await axiosInstance.get(`${apiUrl}/Returns/generateExcel`);
    return ID;
  };

  if (
    !accounts || !Object.keys(accounts).length ||
    !individualAccountData ||
    Object.keys(individualAccountData).length === 0
  ) {
    return <p>Loading returns...</p>; //
  }

  return (
    <>
      <div className="Home__account-wrapper">
        <h2>Accounts</h2>
        <button onClick={(x) => generateExcel()}>Generate Excel Report</button>
      </div>
      {/* Loop creating column names */}
      <div className="Home__account-wrapper">
        <p className="Home__account-column">Account Name</p>
        <p className="Home__year-column">Jan 2021</p>
        {yearsArray.map((year) => (
          <p key={year} className="Home__year-column">
            {year}
          </p>
        ))}
        <p className="Home__year-column">Since Jan 2021</p>
        {/* <button className="Home__delete-button">XXXXX</button> */}
      </div>
      {/* Loop creating rows showing each year's returns */}
      {accounts.map((account) => (
        <>
          <div key={account.id} className="Home__account-wrapper">
            <p className="Home__account-column">
              {individualAccountData[Number(account.id)]?.accountName}
            </p>
            <p className="Home__year-column-centred">
              {'£' +
                nfObject.format(
                  individualAccountData[Number(account.id)]
                    ?.balancesAndReturnsByYear[2021]?.startingBalance,
                ) || 'N/A'}
            </p>

            {yearsArray.map((year) => (
              <Link
                key={`${account.id}-${year}`}
                className="Home__year-column"
                to={`/accounts/${Number(account.id)}/${year}`}
              >
                <p>
                  {'£' +
                    nfObject.format(
                      individualAccountData[Number(account.id)]
                        ?.balancesAndReturnsByYear[year]?.endBalance,
                    ) || 'N/A'}
                </p>
                <p
                  className={
                    individualAccountData[Number(account.id)]
                      ?.balancesAndReturnsByYear[year].xirr >= 0
                      ? 'Home__positive-data'
                      : 'Home__negative-data'
                  }
                >
                  {individualAccountData[Number(account.id)]
                    ?.balancesAndReturnsByYear[year].xirr + '%' || 'N/A'}
                </p>
              </Link>
            ))}

            <p className="Home__cumret-column">
              {Number(
                nfObject.format(
                  individualAccountData[Number(account.id)]?.cumulativeReturn,
                ),
              ).toFixed(2)}
            </p>

            {/* <button
              className="Home__delete-button"
              onClick={() => toggleDelete(account?.id)}
            >
              {showDelete[account.id] ? 'Cancel' : 'Delete'}
            </button> */}

            {/* <button
              className={
                showDelete[account.id]
                  ? 'Home__delete-button'
                  : 'Home__delete-button--hidden'
              }
              onClick={() => deleteAccount(account?.id)}
            >
              Click here to confirm
            </button> */}
          </div>
        </>
      ))}

      <div className="Home__account-wrapper">
        <p className="Home__account-column">Total</p>
        <p className="Home__year-column">
          {'£' + nfObject.format(aggregateData[2021]?.startBalance) || 'N/A'}
        </p>

        {yearsArray.map((year) => (
          <p key={year} className="Home__year-column">
            {'£' + nfObject.format(aggregateData[year]?.endBalance) || 'N/A'}
          </p>
        ))}

        <p className="Home__year-column">Total</p>
        {/* <button className="Home__delete-button">XXXXX</button> */}
      </div>

      <div className="Home__account-wrapper">
        <p className="Home__account-column">------</p>
        <p className="Home__year-column">{'-------'}</p>

        {yearsArray.map((year) => (
          <p key={year} className="Home__year-column">
            {'-------'}
          </p>
        ))}

        <p className="Home__year-column">Total</p>
        {/* <button className="Home__delete-button">XXXXX</button> */}
      </div>

      <div className="Home__account-wrapper">
        <p className="Home__account-column">Year-on-Year change</p>
        <p className="Home__year-column">{'N/A'}</p>

        {yearsArray.map((year) => (
          <div className="Home__year-column">
            <p
              key={year}
              className={
                aggregateData[year]?.percentChange >= 0
                  ? 'Home__positive-data'
                  : 'Home__negative-data'
              }
            >
              {nfObject.format(aggregateData[year]?.percentChange) + '%' ||
                'N/A'}
            </p>
            <p
              key={year}
              className={
                aggregateData[year]?.percentChange >= 0
                  ? 'Home__positive-data'
                  : 'Home__negative-data'
              }
            >
              {'£' +
                nfObject.format(
                  (
                    (aggregateData[year]?.percentChange *
                      aggregateData[year]?.startBalance) /
                    100
                  ).toFixed(0),
                ) || 'N/A'}
            </p>
          </div>
        ))}

        <p className="Home__year-column">Total</p>
        {/* <button className="Home__delete-button">XXXXX</button> */}
      </div>

      <div className="Home__account-wrapper">
        <p className="Home__account-column">Investment performance</p>
        <p className="Home__year-column">{'N/A'}</p>

        {yearsArray.map((year) => (
          <div className="Home__year-column">
            <p
              key={year}
              className={
                aggregateData[year].xirr >= 0
                  ? 'Home__positive-data'
                  : 'Home__negative-data'
              }
            >
              {nfObject.format(aggregateData[year].xirr) + '%' || 'N/A'}
            </p>
            <p
              key={year}
              className={
                aggregateData[year].xirr >= 0
                  ? 'Home__positive-data'
                  : 'Home__negative-data'
              }
            >
              {'£' +
                nfObject.format(
                  (
                    (aggregateData[year]?.percentChange *
                      aggregateData[year]?.startBalance) /
                      100 -
                    (aggregateData[year]?.percentageNetDeposits *
                      aggregateData[year]?.startBalance) /
                      100
                  ).toFixed(0),
                ) || 'N/A'}
            </p>
          </div>
        ))}

        <p className="Home__year-column">Total</p>
        {/* <button className="Home__delete-button">XXXXX</button> */}
      </div>

      <div className="Home__account-wrapper">
        <p className="Home__account-column">Total cash inflows/outflows</p>
        <p className="Home__year-column">{'N/A'}</p>

        {yearsArray.map((year) => (
          <div className="Home__year-column">
            <p
              key={year}
              className={
                aggregateData[year]?.percentageNetDeposits >= 0
                  ? 'Home__positive-data'
                  : 'Home__negative-data'
              }
            >
              {nfObject.format(aggregateData[year]?.percentageNetDeposits) +
                '%' || 'N/A'}
            </p>
            <p
              key={year}
              className={
                aggregateData[year]?.percentageNetDeposits >= 0
                  ? 'Home__positive-data'
                  : 'Home__negative-data'
              }
            >
              {'£' + nfObject.format(aggregateData[year]?.netDeposits) || 'N/A'}
            </p>
          </div>
        ))}

        <p className="Home__year-column">Total</p>
        {/* <button className="Home__delete-button">XXXXX</button> */}
      </div>

      <div className="Home__account-wrapper">
        <p className="Home__account-column">------</p>
        <p className="Home__year-column">{'-------'}</p>

        {yearsArray.map((year) => (
          <p key={year} className="Home__year-column">
            {'-------'}
          </p>
        ))}

        <p className="Home__year-column">Total</p>
        {/* <button className="Home__delete-button">XXXXX</button> */}
      </div>

      <div className="Home__account-wrapper">
        <p className="Home__account-column">Total Deposits</p>
        <p className="Home__year-column">{'N/A'}</p>

        {yearsArray.map((year) => (
          <div className="Home__year-column">
            <p key={year} className="Home__positive-data">
              {nfObject.format(
                Math.abs(aggregateData[year]?.percentageDeposits),
              ) + '%' || 'N/A'}
            </p>
            <p key={year} className="Home__positive-data">
              {'£' + nfObject.format(aggregateData[year]?.totalDeposits) ||
                'N/A'}
            </p>
          </div>
        ))}

        <p className="Home__year-column">Total</p>
        {/* <button className="Home__delete-button">XXXXX</button> */}
      </div>

      <div className="Home__account-wrapper">
        <p className="Home__account-column">Total Withdrawals</p>
        <p className="Home__year-column">{'N/A'}</p>

        {yearsArray.map((year) => (
          <div className="Home__year-column">
            <p key={year} className="Home__negative-data">
              {nfObject.format(
                Math.abs(aggregateData[year]?.percentageWithdrawals),
              ) + '%' || 'N/A'}
            </p>
            <p key={year} className="Home__negative-data">
              {'£' + nfObject.format(aggregateData[year]?.totalWithdrawals) ||
                'N/A'}
            </p>
          </div>
        ))}

        <p className="Home__year-column">Total</p>
        {/* <button className="Home__delete-button">XXXXX</button> */}
      </div>

      <form onSubmit={(e) => addAccount(e)}>
        <label name="accName">Account Name</label>
        <input name="accName" id="accName" />
        <label name="startVal">Jan 2021 Value</label>
        <input name="startVal" id="startVal" type="number" />

        <button type="submit">Submit</button>
      </form>
    </>
  );
};

export default Home;
